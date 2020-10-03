using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace ScsContentMigrator.Data
{
	public class Checksum
	{
		internal readonly Dictionary<string, SortedSet<string>> _checksumTracker = new Dictionary<string, SortedSet<string>>();
		internal readonly Dictionary<string, string> _revTracker = new Dictionary<string, string>();
		internal readonly Dictionary<string, List<string>> _childTracker = new Dictionary<string, List<string>>();
		internal readonly Dictionary<string,string> _parentTracker = new Dictionary<string, string>();
		internal readonly HashSet<string> _leafTracker = new HashSet<string>();
		internal readonly Dictionary<string, string> _checksum = new Dictionary<string, string>();
		//t.ID, t.Name, t.TemplateID, t.MasterID, t.ParentID, v.Value
		public void LoadRow(string id, string parentId, string value)
		{
			if (_parentTracker.ContainsKey(id)) return;
			_parentTracker[id] = parentId;
			if (!_childTracker.ContainsKey(parentId))
			{
				_childTracker[parentId] = new List<string>();
			}
			_childTracker[parentId].Add(id);
			if (!_checksumTracker.ContainsKey(id))
			{
				_checksumTracker[id] = new SortedSet<string>();
			}
			_revTracker[id] = value;
			if (!_childTracker.ContainsKey(id))
				_leafTracker.Add(id);
			_leafTracker.Remove(parentId);
		}

		public string GetChecksum(string id)
		{
			if (!Guid.TryParse(id, out Guid result)) return null;
			string key = result.ToString();
			if (_checksum.ContainsKey(key))
			{
				//if (id == "{4A08D645-E12D-4AEB-9BB7-F4EE919D8D93}")
					Sitecore.Diagnostics.Log.Info($"Checksum {key} {_checksum[key]}", this);
				return _checksum[key];
			}
			return null;
		}

		public void Generate()
		{
			Queue<string> processing = new Queue<string>(_leafTracker);
			while (processing.Any())
			{
				string id = processing.Dequeue();
				if (_checksumTracker[id].Count == 0)
				{
					if (_checksumTracker.ContainsKey(_parentTracker[id]))
					{
						_checksumTracker[_parentTracker[id]].Add(GetHashCode32(_revTracker[id]+id).ToString());
					}
				}
				else
				{
					_checksum[id] = GetHashCode32(string.Join("", _checksumTracker[id]));
					if (_checksumTracker.ContainsKey(_parentTracker[id]))
					{
						_checksumTracker[_parentTracker[id]].Add(_checksum[id].ToString());
					}
				}
				if (_checksumTracker.ContainsKey(_parentTracker[id]) && _checksumTracker[_parentTracker[id]].Count >= _childTracker[_parentTracker[id]].Count)
				{
					processing.Enqueue(_parentTracker[id]);
				}
			}
			_checksumTracker.Clear();
			_childTracker.Clear();
			_parentTracker.Clear();
			_leafTracker.Clear();
		}
		public string GetHashCode32(string s)
		{
			using (SHA1Managed sha1 = new SHA1Managed())
			{
				var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(s));
				var sb = new StringBuilder(hash.Length * 2);

				foreach (byte b in hash)
				{
					sb.Append(b.ToString("X2"));
				}

				return sb.ToString();
			}
		}
	}
}
