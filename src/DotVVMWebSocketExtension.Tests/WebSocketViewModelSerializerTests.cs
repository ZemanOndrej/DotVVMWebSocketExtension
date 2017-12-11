using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVMWebSocketExtension.WebSocketService;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DotVVMWebSocketExtension.Tests
{
	public class WebSocketViewModelSerializerTests
	{
		public Mock<IViewModelSerializationMapper> mapper { get; set; }
		public Mock<DotvvmRequestContext>context { get; set; }
		public WebSocketViewModelSerializer serializer { get; set; }


		public WebSocketViewModelSerializerTests()
		{
			mapper = new Mock<IViewModelSerializationMapper>();
			context = new Mock<DotvvmRequestContext>();
			serializer = new WebSocketViewModelSerializer(mapper.Object);
		}

		[Fact]
		public void Test()
		{

			throw new NotImplementedException();
		}

		[Fact]
		public void TestSerializeViewModelWithDiff()
		{
			var received = CreateRecieveJObject();
			var currentView = CreateViewModelJObject();

			context.SetupGet(c => c.ViewModelJson).Returns(currentView);
			context.SetupGet(s => s.ReceivedViewModelJson).Returns(received);
			Assert.NotNull(context.Object.ViewModelJson["viewModel"]);

			var str = serializer.SerializeViewModel(context.Object);
			Assert.Equal("{\"action\":\"successfulCommand\",\"viewModelDiff\":{\"Text\":\"Stage 3\"}}", str);
			Assert.Null(context.Object.ViewModelJson["viewModel"]);
			Assert.DoesNotContain("\"viewModel\"", str);
		}


		public JObject CreateRecieveJObject()
		{
			return JObject.Parse(@"{
  ""viewModel"": {
    ""$type"": ""UzHn+11VGfavpqHGE7Du+aGAwok="",
    ""Hub"": {
      ""$type"": ""9wRAK8ilQaKKVYvD6JG40TIN4gU="",
      ""CurrentGroupId"": null,
      ""ConnectionId"": ""c6e8a12f-845e-4df7-8aad-1746ba1c0f99""
    },
    ""Kappucino"": ""Random String"",
    ""Text"": ""Stage 2"",
    ""$csrfToken"": ""CfDJ8OvjlpKWk6BNv5R6oeQf5IFQo+X02TdMC4DtqHiuQV/Uenm75w/5x+2M8BOIPNtl0By7/REABRIsy6I8p9iZf14VJEh7DL+6PfYlLiGhcwoLb5KnRh9kA7yn9bwdDhlAMqBh1QDHqD2qr6P7ViRnNZhAgwUkkhOfgj8fcEBVlwFv""
  },
  ""currentPath"": [],
  ""command"": ""mtnOjJsVLet/DrbT"",
  ""controlUniqueId"": """",
  ""validationTargetPath"": ""dotvvm.viewModelObservables['root']"",
  ""renderedResources"": [
    ""knockout"",
    ""dotvvm.internal"",
    ""dotvvm"",
    ""dotvvm.debug"",
    ""websocketScript""
  ]
}");
		}

		public JObject CreateViewModelJObject()
		{
			return JObject.Parse(@"{
  ""viewModel"": {
    ""$type"": ""UzHn+11VGfavpqHGE7Du+aGAwok="",
    ""Hub"": {
      ""$type"": ""9wRAK8ilQaKKVYvD6JG40TIN4gU="",
      ""CurrentGroupId"": null,
      ""ConnectionId"": ""c6e8a12f-845e-4df7-8aad-1746ba1c0f99""
    },
    ""Kappucino"": ""Random String"",
    ""Text"": ""Stage 3"",
    ""$csrfToken"": ""CfDJ8OvjlpKWk6BNv5R6oeQf5IFQo+X02TdMC4DtqHiuQV/Uenm75w/5x+2M8BOIPNtl0By7/REABRIsy6I8p9iZf14VJEh7DL+6PfYlLiGhcwoLb5KnRh9kA7yn9bwdDhlAMqBh1QDHqD2qr6P7ViRnNZhAgwUkkhOfgj8fcEBVlwFv""
  },
  ""action"": ""successfulCommand""
}");
		}
	}
}