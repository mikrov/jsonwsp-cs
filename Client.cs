using System;
using System.Collections;
using System.Collections.Generic;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System.Net;
using System.IO;
using System.Text;

namespace JsonWsp {
	public class Client{
	
		private string m_service_url_from_description;
		public string m_service_url;
		public string m_description_url;
		
		public Client (string description_url) {
			m_description_url = description_url;
			JsonWsp.Response resp = SendRequest(description_url);
			if (resp.GetJsonWspType()==JsonWsp.Response.JsonWspType.Description) {
				JsonObject json_response = (JsonObject) JsonConvert.Import(resp.GetResponseText());
				m_service_url = (string) json_response["url"];
				m_service_url_from_description = m_service_url;
			}
		}
		
		public void SetViaProxy(bool enable) {
			if (enable) {
				string[] tmp = m_description_url.Split('/');
				List<string> url_parts = new List<string>();
				foreach (string part in tmp) {
					url_parts.Add(part);
				}
				Console.WriteLine(url_parts);
				url_parts.RemoveAt(url_parts.Count-1);
				Console.WriteLine(url_parts);
				m_service_url = String.Join("/",url_parts);
				Console.WriteLine(m_service_url);
			}
			else {
				m_service_url = m_service_url_from_description;
			}
		}

		public static JsonWsp.Response SendRequest(string url, string data="", string content_type="application/json; charset=utf-8") {
			// Send request
			WebRequest request = WebRequest.Create(url);
			
			byte[] byteArray = Encoding.UTF8.GetBytes (data);
			request.ContentType = content_type;
			request.ContentLength = byteArray.Length;
			Stream dataStream;
			if (byteArray.Length>0) {
				// Data avaliable to be posted
				request.Method = "POST";
				dataStream = request.GetRequestStream();
				dataStream.Write (byteArray, 0, byteArray.Length);
				dataStream.Close ();
			}
			else {
				// No data avaliable to be posted
				request.Method = "GET";
			}
			
			WebResponse http_response = request.GetResponse();
			JsonWsp.Response json_response = new JsonWsp.Response(http_response);
			http_response.Close();
			
			return json_response;
		}

		public JsonWsp.Response CallMethod(string methodname,JsonObject args) {
			JsonObject req_dict = new JsonObject();
			req_dict.Add("methodname",methodname);
			req_dict.Add("type","jsonwsp/request");
			req_dict.Add("args",args);
			JsonWriter json_req_writer = new JsonTextWriter();
			req_dict.Export(json_req_writer);
			JsonWsp.Response jsonwsp_response = SendRequest(m_service_url,json_req_writer.ToString());
			// Convert response text
			return jsonwsp_response;
		}
	}
}

