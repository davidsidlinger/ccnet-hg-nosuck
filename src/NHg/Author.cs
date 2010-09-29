using System;
using System.Linq;

namespace NHg
{
	public class Author
	{
		private readonly string _email;
		private readonly string _name;

		public Author(string name, string email)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentOutOfRangeException("name", name, "Name cannot be null or empty.");
			}
			if (email == null)
			{
				throw new ArgumentNullException("email");
			}

			_name = name;
			_email = email;
		}

		public string Name
		{
			get { return _name; }
		}

		public string Email
		{
			get { return _email; }
		}
	}
}
