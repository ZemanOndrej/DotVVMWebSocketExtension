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

//		public void BuildViewModel(Connection connection)
//		{
//			var jsonSerializer = CreateJsonSerializer();
//			var viewModelConverter = new ViewModelJsonConverter(true, mapper)
//			{
//				UsedSerializationMaps = new HashSet<ViewModelSerializationMap>()
//			};
//			jsonSerializer.Converters.Add(viewModelConverter);
//			var writer = new JTokenWriter();
//			try
//			{
//				jsonSerializer.Serialize(writer, connection.LastSentViewModel);
//			}
//			catch (Exception ex)
//			{
//				throw new Exception(
//					$"Could not serialize viewModel of type {connection.LastSentViewModel.GetType().Name}. Serialization failed at property {writer.Path}.",
//					ex);
//			}
//
////			writer.Token["$csrfToken"] = context.CsrfToken;
//
//
//			var result = new JObject();
//			result["viewModel"] = writer.Token;
//			result["action"] = "successfulCommand";
//			connection.ChangedViewModelJson = result;
//		}

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
			if (context.CsrfToken != null)
			{
				writer.Token["$csrfToken"] = context.CsrfToken;
			}

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


//		public string SerializeViewModel(Connection connection)
//		{
//			if (connection.LastSentViewModelJson != null && connection.ChangedViewModelJson["viewModel"] != null)
//			{
//				connection.ChangedViewModelJson?.Remove("viewModelDiff");
//
//				connection.ChangedViewModelJson["viewModelDiff"] = JsonUtils.Diff(
//					(JObject) connection.LastSentViewModelJson["viewModel"],
//					(JObject) connection.ChangedViewModelJson["viewModel"], true);
//
//
//				connection.LastSentViewModelJson["viewModel"] = (JObject) connection.ChangedViewModelJson["viewModel"].DeepClone();
//
//				connection.ChangedViewModelJson.Remove("viewModel");
//			}
//			return connection.ChangedViewModelJson.ToString(JsonFormatting);
//		}

		public string SerializeViewModel(IDotvvmRequestContext context)
		{
			if (context.ReceivedViewModelJson != null && context.ViewModelJson["viewModel"] != null)
			{
				context.ViewModelJson["viewModelDiff"] = JsonUtils.Diff((JObject) context.ReceivedViewModelJson["viewModel"],
					(JObject) context.ViewModelJson["viewModel"], true);

				context.ReceivedViewModelJson = (JObject) context.ViewModelJson.DeepClone();

				context.ViewModelJson.Remove("viewModel");
			}
			return context.ViewModelJson.ToString(JsonFormatting);
		}
	}
}