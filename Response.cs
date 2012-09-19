using System;
using System.Collections;
using System.Net;
using System.IO;
using System.Text;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace JsonWsp {

	public class Fault {
		public enum FaultType {ServerFault,ClientFault};
		public string m_details;
		public string m_error_string;
		public FaultType m_fault_type;
		public string m_filename;
		public long m_lineno;
		
		public Fault (JsonObject fault) {
			m_details = String.Join("\n",(JsonArray) fault["detail"]);
			m_error_string = (string) fault["string"];
			m_fault_type = (string)fault["code"]=="server"?FaultType.ServerFault:FaultType.ClientFault;
			m_filename = (string)fault["filename"];
			m_lineno = ((JsonNumber)fault["lineno"]).ToInt32();
		}
		
		public FaultType GetFaultType() {
			return m_fault_type;
		}
		public string GetString() {
			return m_error_string;
		}
		public string GetDetails() {
			return m_details;
		}
		public string GetFilename() {
			return m_filename;
		}
		public long GetLineNo() {
			return m_lineno;
		}
	}

	public class Response {

		public enum CallResult {NetworkFault,ServiceFault,Success};
		public enum JsonWspType {NoType,Description,Response,Fault};
		public int m_status_code;
		public string m_status_desc;
		public string m_response_text;
		public JsonObject m_json_response;
		public JsonWspType m_jsonwsp_type;
		public CallResult m_call_result;
		public Fault m_fault;

		
		public Response (WebResponse response) {
			m_status_code = (int) ((HttpWebResponse)response).StatusCode;
			m_status_desc = ((HttpWebResponse)response).StatusDescription;
			// Get the response.
			Stream dataStream;
			dataStream = response.GetResponseStream ();
			// Open the stream using a StreamReader for easy access.
			StreamReader reader = new StreamReader (dataStream);
			// Read the content.
			m_response_text = reader.ReadToEnd ();
			// Clean up the streams.
			reader.Close();
			dataStream.Close();
			if (m_status_code==200) {
				m_json_response = (JsonObject) JsonConvert.Import(m_response_text);
				if ((string)m_json_response["type"]=="jsonwsp/description") {
					m_jsonwsp_type = JsonWspType.Description;
					m_call_result = CallResult.Success;
				}
				if ((string)m_json_response["type"]=="jsonwsp/response") {
					m_jsonwsp_type = JsonWspType.Response;
					m_call_result = CallResult.Success;
				}
				if ((string)m_json_response["type"]=="jsonwsp/fault") {
					m_jsonwsp_type = JsonWspType.Fault;
					m_call_result = CallResult.ServiceFault;
					JsonObject jsonwsp_fault = (JsonObject) m_json_response["fault"];
					m_fault = new Fault(jsonwsp_fault);
				}
			}
			else {
				m_jsonwsp_type = JsonWspType.NoType;
				m_call_result = CallResult.NetworkFault;
			}
		}
		
		public int GetStatusCode() {
			return m_status_code;
		}
		public string GetStatusString() {
			return m_status_desc;
		}
		public string GetResponseText() {
			return m_response_text;
		}
		public JsonObject GetJsonResponse() {
			return m_json_response;
		}
		public CallResult GetCallResult() {
			return m_call_result;
		}
		public JsonWspType GetJsonWspType() {
			return m_jsonwsp_type;
		}
		public Fault GetServiceFault() {
			return m_fault;
		}
	}
}

