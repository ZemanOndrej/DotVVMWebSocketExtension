using System;
using System.Collections.Generic;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketViewModelSerializer
	{
//		private const string GeneralViewModelRecommendations = "Check out general viewModel recommendation at http://www.dotvvm.com/docs/tutorials/basics-viewmodels.";

		protected readonly IViewModelSerializationMapper mapper;

		public Formatting JsonFormatting { get; set; }
//		private readonly IViewModelProtector viewModelProtector;


		public WebSocketViewModelSerializer(IViewModelSerializationMapper mapper)
		{
			this.mapper = mapper;
//			this.viewModelProtector = viewModelProtector;
			JsonFormatting = Formatting.None;
		}

		public void BuildViewModel(IDotvvmRequestContext context)
		{
			var jsonSerializer = CreateJsonSerializer();
			var viewModelConverter = new ViewModelJsonConverter(context.IsPostBack, mapper)
			{
				UsedSerializationMaps = new HashSet<ViewModelSerializationMap>()
			};
			jsonSerializer.Converters.Add(viewModelConverter);
			var writer = new JTokenWriter();
			try
			{
				jsonSerializer.Serialize(writer, context.ViewModel);
			}
			catch (Exception ex)
			{
				throw new Exception(
					$"Could not serialize viewModel of type {context.ViewModel.GetType().Name}. Serialization failed at property {writer.Path}.",
					ex);
			}

			writer.Token["$csrfToken"] = context.CsrfToken;


			var result = new JObject();
			result["viewModel"] = writer.Token;
			result["action"] = "successfulCommand";
			context.ViewModelJson = result;
		}

		protected virtual JsonSerializer CreateJsonSerializer() => CreateDefaultSettings().Apply(JsonSerializer.Create);


		public static JsonSerializerSettings CreateDefaultSettings()
		{
			var s = new JsonSerializerSettings
			{
				DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
			};
			s.Converters.Add(new DotvvmDateTimeConverter());
			s.Converters.Add(new StringEnumConverter());
			return s;
		}


		public string SerializeViewModel(IDotvvmRequestContext context, Connection connection)
		{
			if (context.ReceivedViewModelJson != null && context.ViewModelJson["viewModel"] != null)
			{
				context.ViewModelJson?.Remove("viewModelDiff");

				try
				{
					if (connection.LastSentViewModelJson == null)
					{
						context.ViewModelJson["viewModelDiff"] = JsonUtils.Diff((JObject) context.ReceivedViewModelJson["viewModel"],
							(JObject) context.ViewModelJson["viewModel"], true);
						connection.LastSentViewModelJson = new JObject();
					}
					else
					{
						var diff = JsonUtils.Diff((JObject) context.ReceivedViewModelJson["viewModel"],
							(JObject) context.ViewModelJson["viewModel"], true);// TODO tento DIFF dava napicu {} objektiky do predu


						var diff2 = (JObject)connection.LastSentViewModelJson["viewModel"].DeepClone();

						diff2.Merge(diff, new JsonMergeSettings {MergeArrayHandling = MergeArrayHandling.Union});

						var diff3 = JsonUtils.Diff((JObject)connection.LastSentViewModelJson["viewModel"],
							diff2, true);

						context.ViewModelJson["viewModelDiff"] = diff3;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}

//				var o =(JObject) context.ViewModelJson["viewModelDiff"];
//				o.Remove("Hub");
//				o.Remove("$csrfToken");

				connection.LastSentViewModelJson["viewModel"] = (JObject) context.ViewModelJson["viewModel"].DeepClone();

				context.ViewModelJson.Remove("viewModel");
				return context.ViewModelJson.ToString(JsonFormatting);
			}
			return context.ViewModelJson.ToString(JsonFormatting);
		}


		//		public string SerializeModelState(IDotvvmRequestContext context)
		//		{
		//			var result = new JObject();
		//			result["modelState"] = JArray.FromObject(context.ModelState.Errors);
		//			result["action"] = "validationErrors";
		//			return result.ToString(JsonFormatting);
		//		}


		//		public JObject BuildResourcesJson(IDotvvmRequestContext context, Func<string, bool> predicate)
		//		{
		//			var manager = context.ResourceManager;
		//			var resourceObj = new JObject();
		//			foreach (var resource in manager.GetNamedResourcesInOrder())
		//			{
		//				if (predicate(resource.Name))
		//				{
		//					using (var str = new StringWriter())
		//					{
		//						resourceObj[resource.Name] = JValue.CreateString(resource.GetRenderedTextCached(context));
		//					}
		//				}
		//			}
		//			return resourceObj;
		//		}


		//		public void PopulateViewModel(IDotvvmRequestContext context, string serializedPostData)
		//		{
		//
		//			// get properties
		//			var data = context.ReceivedViewModelJson = JObject.Parse(serializedPostData);
		//			var viewModelToken = (JObject)data["viewModel"];
		//
		//			// load CSRF token
		//			context.CsrfToken = viewModelToken["$csrfToken"].Value<string>();
		//
		//			ViewModelJsonConverter viewModelConverter;
		//			if (viewModelToken["$encryptedValues"] != null)
		//			{
		//				// load encrypted values
		//				var encryptedValuesString = viewModelToken["$encryptedValues"].Value<string>();
		//				viewModelConverter = new ViewModelJsonConverter(context.IsPostBack, mapper, JObject.Parse(viewModelProtector.Unprotect(encryptedValuesString, context)));
		//			}
		//			else viewModelConverter = new ViewModelJsonConverter(context.IsPostBack, mapper);
		//
		//			// get validation path
		//			context.ModelState.ValidationTargetPath = data.SelectToken("additionalData.validationTargetPath")?.Value<string>();
		//
		//			// populate the ViewModel
		//			var serializer = CreateJsonSerializer();
		//			serializer.Converters.Add(viewModelConverter);
		//			try
		//			{
		//				viewModelConverter.Populate(viewModelToken.CreateReader(), serializer, context.ViewModel);
		//			}
		//			catch (Exception ex)
		//			{
		//				throw new Exception($"Could not deserialize viewModel of type { context.ViewModel.GetType().Name }. {GeneralViewModelRecommendations}", ex);
		//			}
		//		}
	}
}