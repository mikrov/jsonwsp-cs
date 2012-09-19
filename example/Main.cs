using System;
using System.Collections;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using JsonWsp;
using System.IO;
using System.Net;
using System.Text;

namespace jsontest
{
	class MainClass
	{
		public static JsonObject CommonCall(JsonWsp.Client cli, string method_name, JsonObject args_dict) {
			// Call method
			JsonWsp.Response response = cli.CallMethod(method_name,args_dict);
			// Print
			JsonWsp.Response.CallResult cr = response.GetCallResult();
			JsonObject result;
			if (cr==JsonWsp.Response.CallResult.Success) {
				result = (Jayrock.Json.JsonObject) response.GetJsonResponse()["result"];
				JsonObject method_result = (JsonObject) result["method_result"];
				Console.WriteLine("RES_CODE: " + method_result["res_code"]);
				Console.WriteLine("RES_MSG:  " + method_result["res_msg"]);
				Console.WriteLine("----------");
				return result;
			}
			if (cr==JsonWsp.Response.CallResult.ServiceFault) {
				Console.WriteLine("ERROR STRING: " + response.GetServiceFault().GetString());
				Console.WriteLine("ERROR LINENO: " + response.GetServiceFault().GetLineNo());
				Console.WriteLine("----------");
			}
			return null;
		}
		
		public static void Main (string[] args)
		{
			JsonWsp.Client cli = new JsonWsp.Client("http://10.1.5.160/service/AccessService/jsonwsp/description");
			cli.SetViaProxy(true);
			if (args.Length==0) {
				Console.WriteLine("Usage: jsontest.exe <session_id>");
				return;
			}
			Console.WriteLine("Call listPermissions");
			Console.WriteLine("--------------------");
			// Build arguments
			JsonObject args_dict = new JsonObject();
			args_dict.Add("session_id",args[0]);
			// Call method
			JsonObject result = CommonCall(cli,"listPermissions",args_dict);
			if (result != null) {
				Jayrock.Json.JsonArray perm = (Jayrock.Json.JsonArray) result["access_identifiers"];
				foreach (string i in perm) {
					Console.WriteLine(i);
				}
			}

			Console.WriteLine();
			Console.WriteLine("Call hasPermissions");
			Console.WriteLine("-------------------");
			// Build arguments
			JsonObject args_dict2 = new JsonObject();
			args_dict2.Add("session_id",args[0]);
			args_dict2.Add("access_identifier","product.web.10finger");
			// Call method
			result = CommonCall(cli,"hasPermission",args_dict2);
			// Print
			if (result != null) {
				bool access = (bool) result["has_permission"];
				if (access) {
					Console.WriteLine("Access Granted");
				}
				else {
					Console.WriteLine("Access Denied");
				}
			}
			
			Console.WriteLine();
			Console.WriteLine("Call getUserInfo");
			Console.WriteLine("----------------");
			cli = new JsonWsp.Client("http://10.1.5.160/service/UserService/jsonwsp/description");
			result = CommonCall(cli,"getUserInfo",args_dict);
			
			if (result != null) {
				JsonObject user_info = (JsonObject) result["user_info"];
				Console.WriteLine("Org-name:    " + user_info["org_name"]);
				Console.WriteLine("Org-domain:  " + user_info["org_domain"]);
				Console.WriteLine("Org-code:    " + user_info["org_code"]);
				Console.WriteLine("Org-type:    " + user_info["org_type"]);
				Console.WriteLine("Given Name:  " + user_info["given_name"]);
				Console.WriteLine("Surname:     " + user_info["surname"]);
				Console.WriteLine("Uid:         " + user_info["uid"]);
				Console.WriteLine("Common-name: " + user_info["common_name"]);
			}
		}
	}
}

