using System;
using System.Collections.Generic;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public class MediaComparer : IEqualityComparer<MediaAutoCompleteModel>
    {
        // Media are equal if their names and product numbers are equal.
        public bool Equals(MediaAutoCompleteModel x, MediaAutoCompleteModel y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (x is null || y is null)
                return false;

            //Check whether the products' properties are equal.
            return x.Name == y.Name;
        }

        public int GetHashCode(MediaAutoCompleteModel media)
        {
            //Check whether the object is null
            if (media is null) return 0;

            //Calculate the hash code for the product.
            return media.Name == null ? 0 : media.Name.GetHashCode();
        }

    }
}
