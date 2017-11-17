using System;
using System.Collections.Generic;
using System.IO;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketViewModelSerializer
	{

		protected readonly IViewModelSerializationMapper mapper;
		public Formatting JsonFormatting { get; set; }
		public bool SendDiff { get; set; } = true;



		public WebSocketViewModelSerializer(IViewModelSerializationMapper mapper)
		{
			this.mapper = mapper;
			JsonFormatting =  Formatting.None;
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
			var s = new JsonSerializerSettings()
			{
				DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
			};
			s.Converters.Add(new DotvvmDateTimeConverter());
			s.Converters.Add(new StringEnumConverter());
			return s;
		}


		public string SerializeViewModel(IDotvvmRequestContext context)
		{
			if (SendDiff && context.ReceivedViewModelJson != null && context.ViewModelJson["viewModel"] != null)
			{
				context.ViewModelJson["viewModelDiff"] = JsonUtils.Diff((JObject)context.ReceivedViewModelJson["viewModel"], (JObject)context.ViewModelJson["viewModel"], true);
				context.ViewModelJson.Remove("viewModel");
			}
			return context.ViewModelJson.ToString(JsonFormatting);
		}

		public string SerializeModelState(IDotvvmRequestContext context)
		{
			var result = new JObject();
			result["modelState"] = JArray.FromObject(context.ModelState.Errors);
			result["action"] = "validationErrors";
			return result.ToString(JsonFormatting);
		}

		

		public JObject BuildResourcesJson(IDotvvmRequestContext context, Func<string, bool> predicate)
		{
			var manager = context.ResourceManager;
			var resourceObj = new JObject();
			foreach (var resource in manager.GetNamedResourcesInOrder())
			{
				if (predicate(resource.Name))
				{
					using (var str = new StringWriter())
					{
						resourceObj[resource.Name] = JValue.CreateString(resource.GetRenderedTextCached(context));
					}
				}
			}
			return resourceObj;
		}
	}
}