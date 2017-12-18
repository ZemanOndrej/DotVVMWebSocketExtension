using System.Collections.Generic;

namespace BL.DTO
{
	public class UserDto
	{
		public int Id { get; set; }
		public string ConnectionId { get; set; }
		public string Name { get; set; }
		public int CurrentRoomId { get; set; }

		private sealed class IdEqualityComparer : IEqualityComparer<UserDto>
		{
			public bool Equals(UserDto x, UserDto y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null)) return false;
				if (ReferenceEquals(y, null)) return false;
				if (x.GetType() != y.GetType()) return false;
				return x.Id == y.Id;
			}

			public int GetHashCode(UserDto obj)
			{
				return obj.Id;
			}
		}

		public static IEqualityComparer<UserDto> IdComparer { get; } = new IdEqualityComparer();
	}
}