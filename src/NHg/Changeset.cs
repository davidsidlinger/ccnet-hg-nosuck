using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using log4net;

namespace NHg
{
	public class Changeset
	{
		public const string RevisionAttributeName = "revision";
		public const string AuthorElementName = "author";
		public const string EmailAttributeName = "email";
		public const string MessageElementName = "msg";
		public const string DateElementName = "date";
		private const string ActionAttributeName = "action";
		private const string PathElementName = "path";
		private const string ParentElementName = "parent";
		public static readonly string HashAttributeName = "node";
		private static readonly ILog Log = LogManager.GetLogger(typeof(Changeset));
		private readonly Author _author;
		private readonly DateTimeOffset _date;
		private readonly Hash _hash;
		private readonly string _message;
		private readonly ICollection<ChangesetPath> _paths;
		private readonly int _revisionNumber;
		private readonly ICollection<Hash> _parentHashes;

		public Changeset(Author author,
		                 Hash hash,
		                 int revisionNumber,
		                 DateTimeOffset date,
		                 string message,
		                 IEnumerable<ChangesetPath> paths,
		                 IEnumerable<Hash> parentHashes)
		{
			if (author == null)
			{
				throw new ArgumentNullException("author");
			}
			if (hash == null)
			{
				throw new ArgumentNullException("hash");
			}
			if (paths == null)
			{
				throw new ArgumentNullException("paths");
			}
			if (paths.Any(path => path == null))
			{
				throw new ArgumentNullException("paths");
			}
			if (parentHashes == null)
			{
				throw new ArgumentNullException("parentHashes");
			}
			if (parentHashes.Any(parentHash => parentHash == null))
			{
				throw new ArgumentNullException("parentHashes");
			}

			_author = author;
			_hash = hash;
			_revisionNumber = revisionNumber;
			_date = date;
			_message = message ?? "No changeset message. Naughty.";
			_paths = new List<ChangesetPath>(paths).AsReadOnly();
			_parentHashes = new List<Hash>(parentHashes).AsReadOnly();

			Log.DebugFormat(CultureInfo.CurrentCulture, "Created Changeset for '{0}'.", Hash);
		}

		public Author Author
		{
			get { return _author; }
		}

		public Hash Hash
		{
			get { return _hash; }
		}

		public int RevisionNumber
		{
			get { return _revisionNumber; }
		}

		public DateTimeOffset Date
		{
			get { return _date; }
		}

		public string Message
		{
			get { return _message; }
		}

		public ICollection<ChangesetPath> Paths
		{
			get { return _paths; }
		}

		public ICollection<Hash> ParentHashes
		{
			get { return _parentHashes; }
		}

		public static Changeset FromLogEntryElement(XElement logEntry)
		{
			if (logEntry == null)
			{
				throw new ArgumentNullException("logEntry");
			}
			if (logEntry.Name != HgXmlLogParser.ChangesetName)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
				                                          "Expected element with name '{0}'.",
				                                          HgXmlLogParser.ChangesetName));
			}

			var hash = GetHash(logEntry);
			var revisionNumber = GetRevisionNumber(logEntry);
			var author = GetAuthor(logEntry);
			var date = GetDate(logEntry);
			var message = GetMessage(logEntry);
			var paths = GetPaths(logEntry);
			var parentHashes = GetParentHashes(logEntry);
			return new Changeset(author, hash, revisionNumber, date, message, paths, parentHashes);
		}

		private static IEnumerable<Hash> GetParentHashes(XContainer logEntry)
		{
			Debug.Assert(logEntry != null);

			return logEntry.Descendants(ParentElementName)
				.Select(parent => new Hash(parent.Attribute(HashAttributeName).Value));
		}

		private static IEnumerable<ChangesetPath> GetPaths(XContainer logEntry)
		{
			Debug.Assert(logEntry != null);

			return logEntry.Descendants(PathElementName)
				.Select(path =>
				        {
				        	var action = ChangeAction.Unknown;
				        	switch (path.Attribute(ActionAttributeName).Value)
				        	{
				        		case "A":
				        			action = ChangeAction.Add;
				        			break;
				        		case "M":
				        			action = ChangeAction.Modify;
				        			break;
				        		case "R":
				        			action = ChangeAction.Remove;
				        			break;
				        	}
				        	return new ChangesetPath(action, path.Value);
				        });
		}

		private static string GetMessage(XContainer logEntry)
		{
			Debug.Assert(logEntry != null);
			var messageElement = logEntry.Element(MessageElementName);
			if (messageElement == null)
			{
				Log.Warn("Message element is missing.");
			}
			return messageElement == null ? "" : messageElement.Value;
		}

		private static DateTimeOffset GetDate(XContainer logEntry)
		{
			Debug.Assert(logEntry != null);
			var dateElement = logEntry.Element(DateElementName);
			if (dateElement == null)
			{
				Log.Warn("Date element is missing.");
			}
			DateTimeOffset date;
			if (dateElement == null ||
			    !DateTimeOffset.TryParse(dateElement.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
			{
				date = DateTimeOffset.Now;
				Log.Warn("Date element content could not be parsed.");
			}
			return date;
		}

		private static Author GetAuthor(XContainer logEntry)
		{
			Debug.Assert(logEntry != null);
			string name;
			string email;
			var authorElement = logEntry.Element(AuthorElementName);
			if (authorElement != null)
			{
				name = authorElement.Value;
				var emailAttribute = authorElement.Attribute(EmailAttributeName);
				email = emailAttribute == null ? "" : emailAttribute.Value;
			}
			else
			{
				Log.Warn("Author element missing.");
				name = "";
				email = "";
			}
			return new Author(name, email);
		}

		private static int GetRevisionNumber(XElement logEntry)
		{
			Debug.Assert(logEntry != null);
			var revisionAttribute = logEntry.Attribute(RevisionAttributeName);
			var revisionNumber = 0;
			if (revisionAttribute == null || !int.TryParse(revisionAttribute.Value,
			                                               NumberStyles.Integer,
			                                               CultureInfo.InvariantCulture,
			                                               out revisionNumber))
			{
				Log.Warn("Could not find or parse revision number.");
			}
			return revisionNumber;
		}

		private static Hash GetHash(XElement logEntry)
		{
			Debug.Assert(logEntry != null);
			var hashAttribute = logEntry.Attribute(HashAttributeName);
			if (hashAttribute == null)
			{
				throw new ArgumentException("'node' attribute missing from element.", "logEntry");
			}
			return new Hash(hashAttribute.Value);
		}
	}
}
