using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public static class EnumExtensions {
        //T should be enum but impossible to give such constraint. Using:
        //http://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
        //but thrown out IConvertible doesn't seem to work in bridge.net
        public static IEnumerable<T> GetEnumValues<T>() where T:struct {
            return new List<T>((T[])Enum.GetValues(typeof(T)));
        }

        //T should be enum but impossible to give such constraint. Using:
        //http://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
        //but thrown out IConvertible doesn't seem to work in bridge.net
        public static List<string> GetLabelsHavingValue<T>(int value, Func<T,int> asValue) where T:struct {
            return Enum.GetNames(typeof(T))
                .Where(x => asValue(GetEnumByLabel<T>(x)) == value)
                .ToList();
        }

        public static IEnumerable<T> GetUniqueEnumValues<T>(Func<T,int> enumToInt) where T:struct {
            var found = new HashSet<int>();
            return GetEnumValues<T>().Where(x => {
                var num = enumToInt(x);

                if (found.Contains(num)) {
                    return false;
                }

                found.Add(num);
                return true;
            }).ToList();
        }

        //T should be enum but impossible to give such constraint. Using:
        public static T GetEnumByLabel<T>(string inp) where T:struct {
            //bridge bug: 
            //in bridge.net it returns object
            //in .net framework it is Enum
            object obj = Enum.Parse(typeof(T), inp);
            return (T)obj;
        }
    }
}
