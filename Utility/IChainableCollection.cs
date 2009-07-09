using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace JawTek.Web.Utility
{
    public interface IChainableCollection<T> : ICollection<T>, IEnumerable
    {
        new IChainableCollection<T> Add(T item);
        new IChainableCollection<T> Clear();
        new IChainableCollection<T> CopyTo(T[] array, int arrayindex);
    }
}
