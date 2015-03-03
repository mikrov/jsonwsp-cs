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
        public Dictionary<String, String> m_cookieList = null;
        public Dictionary<String, String> m_headerList = null;

        public Client(string description_url, Dictionary<String, String> cookies = null, Dictionary<String, String> headers = null)
        {

            if (cookies == null)
            {
                m_cookieList = new Dictionary<string, string>();
            }
            else
            {
                m_cookieList = cookies;
            }

            if (headers == null)
            {
                m_headerList = new Dictionary<string, string>();
            }
            else
            {
                m_headerList = headers;
            }

			m_description_url = description_url;
            string content_type = "application/json; charset=utf-8";
            JsonWsp.Response resp = SendRequest(description_url,"",content_type, m_cookieList, m_headerList);
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
				url_parts.RemoveAt(url_parts.Count-1);
				m_service_url = String.Join("/",url_parts.ToArray());
			}
			else {
				m_service_url = m_service_url_from_description;
			}
		}

        public void AddCookie(String name, String value)
        {
            if (m_cookieList.ContainsKey(name))
            {
                m_cookieList[name] = value;
            }
            else
            {
                m_cookieList.Add(name, value);
            }
        }

        public void RemoveCookie(String name)
        {
            if (m_cookieList.ContainsKey(name))
            {
                m_cookieList.Remove(name);
            }
        }

        public void AddHeader(String name, String value)
        {
            if (m_headerList.ContainsKey(name))
            {
                m_headerList[name] = value;
            }
            else
            {
                m_headerList.Add(name, value);
            }
        }

        public void RemoveHeader(String name)
        {
            if (m_headerList.ContainsKey(name))
            {
                m_headerList.Remove(name);
            }
        }

        public static JsonWsp.Response SendRequest(string url, string data, string content_type, Dictionary<String, String> cookies = null, Dictionary<String, String> headers = null)
        {
			// Send request
			WebRequest request = WebRequest.Create(url);
			
			byte[] byteArray = Encoding.UTF8.GetBytes (data);
			request.ContentType = content_type;
			request.ContentLength = byteArray.Length;

            // Add cookies to request
            if (cookies.Count > 0)
            {
                string cookieString = "";
                foreach (String name in cookies.Keys)
                {
                    cookieString += (cookieString != "" ? "; " : "")+name+"="+cookies[name];
                }
                request.Headers.Add("Cookie: " + cookieString);
            }

            // Add headers to request
            if (headers.Count > 0)
            {
                foreach (String name in headers.Keys)
                {
                    request.Headers.Add(name+": " + headers[name]);
                }   
            }
            
			Stream dataStream;
			if (byteArray.Length>0) {
				// Data avaliable to be posted
				request.Method = "POST";
				dataStream = request.GetRequestStream();
				dataStream.Write (byteArray, 0, byteArray.Length);
				dataStream.Close ();
			}
			else 
            {
				// No data avaliable to be posted
				request.Method = "GET";
			}
			
			WebResponse http_response = request.GetResponse();
			JsonWsp.Response json_response = new JsonWsp.Response(http_response);
			http_response.Close();
			
			return json_response;
		}
	
	    public static JsonWsp.Response SendRequest(string url, string data, Dictionary<String,String> cookies=null)
	    {
	        string content_type = "application/json; charset=utf-8";
	        return SendRequest(url, data, content_type);
	    }

        public static JsonWsp.Response SendRequest(string url, Dictionary<String, String> cookies = null)
	    {
	        string content_type = "application/json; charset=utf-8";
	        string data = "";
	        return SendRequest(url, data, content_type);
	    }


        public JsonWsp.Response CallMethod(string methodname, JsonObject args, Dictionary<String, String> cookies = null, Dictionary<String, String> headers = null)
        {
			JsonObject req_dict = new JsonObject();
			req_dict.Add("methodname",methodname);
			req_dict.Add("type","jsonwsp/request");
			req_dict.Add("args",args);
			JsonWriter json_req_writer = new JsonTextWriter();
			req_dict.Export(json_req_writer);

            Dictionary<String, String> cookieList = m_cookieList;
            if (cookies != null)
            {
                foreach (String key in cookies.Keys)
                {
                    cookieList[key] = cookies[key];
                }
            }

            Dictionary<String, String> headerList = m_headerList;
            if (headers != null)
            {
                foreach (String key in headers.Keys)
                {
                    headerList[key] = headers[key];
                }
            }

            string content_type = "application/json; charset=utf-8";
            JsonWsp.Response jsonwsp_response = SendRequest(m_service_url, json_req_writer.ToString(),content_type, cookieList, headerList);
			// Convert response text
			return jsonwsp_response;
		}
	}
}

