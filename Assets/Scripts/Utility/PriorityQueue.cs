// PriorityQueue.cs
// by: Peter Bartosch <bartoschp@gmail.com>
//
// Defines a priority queue data structure.  This is used in pathfinding, but may have
// a use elsewhere.
//
// TODO
// * Consider making the priority queue have a custom sort, so that highest values can
//   be at the front.
// * Actually test the _noExpand flag.
// * The heap aspects could be commented better.
//
using System;
using System.Collections;
using System.Collections.Generic;

namespace Inkspot.Utility
{
	/// <summary>
	/// A priority queue that puts the element with lowest value at the front.
	/// </summary>
	public class PriorityQueue<T> : ICollection, ICollection<T> where T : IComparable, IComparable<T>
	{
		#region fields
		private T[] _array; //the underlying array for the heap
		private int _size; //the size of the array
		private int _count; //the number of elements currently stored
		private bool _noExpand; //whether or not this can increase in size
		#endregion
		
		#region ctors
		/// <summary>
		/// Initializes a new instance of the <see cref="Inkspot.Utility.PriorityQueue`1"/> class.
		/// </summary>
		public PriorityQueue()
			: this(1000, false)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="Inkspot.Utility.PriorityQueue`1"/> class.
		/// </summary>
		/// <param name="size">The starting size of the underlying array.</param>
		public PriorityQueue(int size)
			: this(size, false)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="Inkspot.Utility.PriorityQueue`1"/> class.
		/// </summary>
		/// <param name="size">The starting size of the underlying array.</param>
		/// <param name="noExpand">If set to <c>true</c> the priority queue will not expand in size.</param>
		public PriorityQueue(int size, bool noExpand)
		{
			_array = new T[size];
			_size = size;
			_count = 0;
			_noExpand = noExpand;
		}
		#endregion

		#region IEnumerable<T> implementation
		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="PriorityQueue{T}"/>.
		/// </summary>
		/// <returns>A <see cref="IEnumerator{T}"/> that can be used to iterate through the <see cref="PriorityQueue{T}"/>.</returns>
		/// <exception cref="NotImplementedException">
		/// This is thrown because I'm not yet sure as to how to implement the enumeration
		/// of the <see cref="PriorityQueue{T}"/>, if at all.
		/// </exception>
		public IEnumerator<T> GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="PriorityQueue{T}"/>.
		/// </summary>
		/// <returns>A <see cref="IEnumerator"/> that can be used to iterate through the <see cref="PriorityQueue{T}"/>.</returns>
		/// <exception cref="NotImplementedException">
		/// This is thrown because I'm not yet sure as to how to implement the enumeration
		/// of the <see cref="PriorityQueue{T}"/>, if at all.
		/// </exception>
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region ICollection<T> implementation
		/// <summary>
		/// Adds an item to the <see cref="PriorityQueue{T}"/>.
		/// </summary>
		/// <param name="item">The object to add.</param>
		/// <remarks>
		/// This will automatically resize the <see cref="PriorityQueue{T}"/> if it hits it's
		/// max size.  If it cannot be resized, then nothing is added and this returns.  This
		/// will automatically insert <paramref name="item"/> into the correct location.
		/// </remarks>
		public void Add(T item)
		{
			//if we cannot expand and we're at the limit,
			//return silently.
			if (!_noExpand && _count == _size)
				increaseSize();
			else if (_noExpand && _count == _size)
				return;
			
			int idx = _count++;
			int parent = parentIndex(idx);
			
			_array[idx] = item;

			//place the new item in the proper spot in the underlying array
			while(idx > 0 && _array[idx].CompareTo(_array[parent]) < 0)
			{
				T tmp = _array[idx];
				_array[idx] = _array[parent];
				_array[parent] = tmp;
				
				idx = parent;
				parent = parentIndex(idx);
			}
		}

		/// <summary>
		/// Removes all items from the <see cref="PriorityQueue{T}"/>.
		/// </summary>
		public void Clear()
		{
			_array = new T[_size];
			_count = 0;
		}
		
		/// <summary>
		/// Checks the <see cref="PriorityQueue"/> for an item.
		/// </summary>
		/// <param name="item">The item to find</param>
		/// <returns><c>true</c> if the item is in the <see cref="PriorityQueue"/>;<c>false</c> otherwise.</returns>
		/// <description>
		/// This just does a linear search since the heap has no guarantee of ordering for traversal.
		/// </description>
		public bool Contains(T item)
		{
			for (int i = 0; i < _count; ++i)
				if (_array[i].CompareTo(item) == 0)
					return true;
			return false;
		}

		/// <summary>
		/// Copies the elements of the <see cref="PriorityQueue{T}"/> to an <see cref="Array"/>,
		/// starting at a particular <see cref="Array"/> index.
		/// </summary>
		/// <param name="array">
		/// The one-dimenstional <see cref="Array"/> that is the destination of the elements copied
		/// from <see cref="PriorityQueue{T}"/>.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">
		/// The number of elements in the source <see cref="PriorityQueue{T}"/> is greater than the available space from
		/// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
		/// </exception>
		/// <remarks>
		/// For our purposes here, this wraps <see cref="Array.CopyTo"/> and rethrows any exceptions it catches.
		/// </remarks>
		public void CopyTo(T[] array, int arrayIndex)
		{
			try
			{
				_array.CopyTo(array, arrayIndex);
			}
			catch //use blank catch so that we catch 'em all.
			{
				//just rethrow any exceptions
				throw;
			}
		}

		/// <summary>
		/// Removes the first occurence of a specific object from the <see cref="PriorityQueue{T}"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="PriorityQueue{T}"/>.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="item"/> was successfully removed from the
		/// <see cref="PriorityQueue{T}"/>; otherwise, false.  This will also return false if
		/// <paramref name="item"/> is not found in the original <see cref="PriorityQueue{T}"/>.
		/// </returns>
		/// <remarks>
		/// We can't just remove <paramref name="item"/> from the underlying array, we have to
		/// re-heapify the <see cref="PriorityQueue{T}"/> once <paramref name="item"/> is removed
		/// to make sure the sorted order remains intact.
		/// </remarks>
		public bool Remove(T item)
		{
			for (int i = 0; i < _count; ++i)
			{
				if (_array[i].CompareTo(item) == 0)
				{
					_array[i] = _array[--_count];
					heapify(i);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="PriorityQueue{T}"/>.
		/// </summary>
		/// <value>The number of elements contained in the <see cref="PriorityQueue{T}"/>.</value>
		public int Count { get { return _count; } }

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>Always <c>false</c>.  A readonly priority queue doesn't make much sense to me.</value>
		public bool IsReadOnly { get { return false; } }
		#endregion

		#region ICollection implementation
		/// <summary>
		/// Copies the elements of the <see cref="PriorityQueue{T}"/> to an <see cref="Array"/>,
		/// starting at a particular <see cref="Array"/> index.
		/// </summary>
		/// <param name="array">
		/// The one-dimenstional <see cref="Array"/> that is the destination of the elements copied
		/// from <see cref="PriorityQueue{T}"/>.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">
		/// The number of elements in the source <see cref="PriorityQueue{T}"/> is greater than the available space from
		/// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
		/// </exception>
		/// <remarks>
		/// This is the non-generic implementation which will box all T into <see cref="System.Object"/>.
		/// For our purposes here, this wraps <see cref="Array.CopyTo"/> and rethrows any exceptions it catches.
		/// </remarks>
		public void CopyTo(Array array, int arrayIndex)
		{
			try
			{
				_array.CopyTo(array, arrayIndex);
			}
			catch //use blank catch so that we catch 'em all.
			{
				//just rethrow any exceptions
				throw;
			}
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="PriorityQueue{T}"/>.
		/// </summary>
		/// <value>The sync root.</value>
		/// <remarks>
		/// I think I understand this on a base level.  Since I'm not working with threads on this one, I'm
		/// not too concerned with a great implementation (so keep that in mind if this ever needs to be
		/// thread safe), but it appears that I just return instance.  See:
		/// http://msdn.microsoft.com/en-us/library/system.collections.icollection.syncroot(v=vs.90).aspx
		/// </remarks>
		public object SyncRoot { get { return this; } }

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="PriorityQueue{T}"/> is sychronized (thread safe).
		/// </summary>
		/// <value>This is currently always false, since I'm not worrying about thread saftey.</value>
		public bool IsSynchronized { get { return false; } }
		#endregion

		#region priority queue functionality
		/// <summary>
		/// Removes and returns the object at the front of the <see cref="PriorityQueue{T}"/>.
		/// </summary>
		/// <returns>
		/// The object that is removed from the front of the <see cref="PriorityQueue{T}"/>.
		/// </returns>
		/// <remarks>
		/// After removing the front element, the entire <see cref="PriorityQueue{T}"/> is
		/// reheapified by pushing the last element all the way back down.  Keep in mind that
		/// <c>null</c> is a valid value for the <see cref="PriorityQueue{T}"/>, so that needs
		/// to be checked when using this.
		/// </remarks>
		/// <exception cref="InvalidOperationException">The <see cref="PriorityQueue{T}"/> is empty.</exception>
		public T Pop()
		{
			if (_count <= 0)
				throw new InvalidOperationException("No element left in the PriorityQueue to pop.");
			
			T ret = _array[0];
			
			//this is the "easiest" way to eliminate the first element, without shifting
			//but this means that the entire heap has to have an element pushed down
			//I'm not sure if it would be better to just shift and then reheapify
			//or do it this way.
			_array[0] = _array[--_count];
			heapify(0);
			return ret;
		}

		/// <summary>
		/// Returns the object at the front of the <see cref="PriorityQueue{T}"/> withiout removing it.
		/// </summary>
		/// <returns>
		/// The object that is removed from the front of the <see cref="PriorityQueue{T}"/>.
		/// </returns>
		/// <remarks>
		/// Keep in mind that <c>null</c> is a valid value for the <see cref="PriorityQueue{T}"/>,
		/// so that needs to be checked when using this.
		/// </remarks>
		/// <exception cref="InvalidOperationException">The <see cref="PriorityQueue{T}"/> is empty.</exception>
		public T Peek()
		{
			if (_count <= 0)
				throw new InvalidOperationException("No element left in the PriorityQueue to peek.");
			
			return _array[0];
		}

		/// <summary>
		/// Finds the specified item in the <see cref="PriorityQueue{T}"/>.
		/// </summary>
		/// <param name="item">The item to find.</param>
		/// <returns>Returns an object equal to <paramref name="item"/> if it is found; the default value of <c>T</c> otherwise.</returns>
		public T Find(T item)
		{
			for(int i=0; i<_count; ++i)
				if(_array[i].CompareTo(item) == 0)
					return _array[i];

			return default(T);
		}

		/// <summary>
		/// Increases the size of the set's underlying array.
		/// </summary>
		private void increaseSize()
		{
			_size = 3 * _size / 2;
			T[] newArray = new T[_size];
			_array.CopyTo(newArray, 0);
			_array = newArray;
		}

		/// <summary>
		/// Heapify starting at the specified idx.
		/// </summary>
		/// <param name="idx">Index.</param>
		/// <remarks>
		/// This will make sure the array is stored as a heap.
		/// </remarks>
		private void heapify(int idx)
		{
			int left = 2 * idx + 1;
			int right = 2 * idx + 2;
			int minIdx = idx;
			
			//keep track by count instead of size to make sure we aren't
			//heapifying things that aren't technically in the heap
			if (left < _count && _array[left].CompareTo(_array[idx]) < 0)
				minIdx = left;
			
			if (right < _count && _array[right].CompareTo(_array[idx]) < 0 && _array[right].CompareTo(_array[left]) < 0)
				minIdx = right;
			
			if(minIdx != idx)
			{
				T tmp = _array[idx];
				_array[idx] = _array[minIdx];
				_array[minIdx] = tmp;
				heapify(minIdx);
			}
			
		}

		/// <summary>
		/// Finds the parents the index.
		/// </summary>
		/// <returns>The index of the parent.</returns>
		/// <param name="idx">The index for which a parent is to be found.</param>
		private int parentIndex(int idx)
		{
			if (idx % 2 == 1)
				return (idx - 1) / 2;
			else
				return (idx - 2) / 2;
		}
		#endregion
	}
}
	