using System;

namespace Inkspot.Pathfinding
{
	[Serializable()]
	/// <summary>
	/// This can be used to represent the cost of a given tag in Unity.  This
	/// is currently unused, but should be used in future versions.
	/// </summary>
	public class TagCost : IComparable, IComparable<TagCost>, IEquatable<TagCost>
	{
		public bool use = false;
		public string tag = "";
		public float cost = 1f;
		
		#region ctors
		/// <summary>
		/// Initializes a new instance of the <see cref="TagCost"/> class.
		/// </summary>
		/// <param name="tag">The tag to make a TagCost for.</param>
		public TagCost(string tag)
		{
			use = false;
			this.tag = tag;
			this.cost = 1f;
		}
		#endregion
		
		#region IComparable implementation
		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether
		/// the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <returns>The result of the comparison</returns>
		/// <param name="obj">The object to compare</param>
		public int CompareTo(System.Object obj)
		{
			if (obj == null) return 1;
			
			TagCost other = obj as TagCost;
			if (other != null)
				return this.CompareTo(other);
			else
				throw new ArgumentException("Attempting to compare a non-TagCost object to a TagCost");
		}
		#endregion
		
		#region IComparable<TagCost> implementation
		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether
		/// the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <returns>The result of the comparison</returns>
		/// <param name="obj">The TagCost to compare</param>
		public int CompareTo(TagCost other)
		{
			if (other != null)
				return tag.CompareTo(other.tag);
			else
				return 1;
		}
		#endregion
		
		#region IEquatable implementation
		/// <summary>
		/// Determines whether the specified <see cref="Inkspot.Pathfinding.TagCost"/> is equal to the current <see cref="Inkspot.Pathfinding.TagCost"/>.
		/// </summary>
		/// <param name="other">The <see cref="Inkspot.Pathfinding.TagCost"/> to compare with the current <see cref="Inkspot.Pathfinding.TagCost"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="Inkspot.Pathfinding.TagCost"/> is equal to the current
		/// <see cref="Inkspot.Pathfinding.TagCost"/>; otherwise, <c>false</c>.</returns>
		public bool Equals(TagCost other)
		{
			return (this.CompareTo(other) == 0);
		}
		#endregion
		
		#region overrides
		/// <summary>
		/// Serves as a hash function for a <see cref="TagCost"/>.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		/// <description>
		/// Since the usage of a TagCost instance is to add data to a unique tag, the hash for the tag string should be sufficient for hashing.
		/// Therefore, this returns the hash for the tag string.
		/// </description>
		public override int GetHashCode()
		{
			return tag.GetHashCode();
		}
		
		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Inkspot.Pathfinding.TagCost"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Inkspot.Pathfinding.TagCost"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="Inkspot.Pathfinding.TagCost"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals(System.Object obj)
		{
			if(obj == null) return false;
			
			TagCost other = obj as TagCost;
			if(other != null) return this.Equals(other);
			else return false;
		}
		#endregion
	}
}