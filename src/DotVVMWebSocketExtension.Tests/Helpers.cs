using DotVVM.Framework.ViewModel;
using Newtonsoft.Json.Linq;

namespace DotVVMWebSocketExtension.Tests
{
	public static class Helpers
	{
		public static JObject CreateRecieveJObject => JObject.Parse(@"{
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

		public static JObject CreateViewModelJObject => JObject.Parse(@"{
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

		public static string SerializedViewModel => JObject.Parse(@"{
				""viewModel"": {
					""RoomName"": ""fsdfsd"",

					""$csrfToken"": ""CfDJ8OvjlpKWk6BNv5R6oeQf5IEfsSKxQJe4LU55dlvgG4wquxzZmz7euS3UeElZGIds3Xre3jsWY8xD49rxPyvVQWnnFuKIky7WnvclncmOtZSYUI5Gyl6T8Aqds8pZEdoHKVt2uNDuOW529j0aU3uP39zp4nL8Hgq8NstF9bIBvP6p""
				},
			}
		").ToString();


	}

	public class TestClass : DotvvmViewModelBase
	{
		public TestClass()
		{
			A = 2;
			B = 32;
		}

		public int A { get; set; }
		public int B { get; set; }
	}

	public class TestViewModel : DotvvmViewModelBase
	{
		public string RoomName { get; set; }
	}
}