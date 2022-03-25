using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.System.Scripts.Localization
{
    public class PluralFormExpressionParser
    {
        private readonly Dictionary<string, Func<int, int>> _mappedMethods;

        //TODO: Pimp this fella later
        public PluralFormExpressionParser()
        {
            _mappedMethods = new Dictionary<string, Func<int, int>>
            {
                {
                    "plural=(n > 1)".Replace(" ", string.Empty),
                    n => n > 1 ? 1 : 0
                },
                {
                    "plural=(n != 1)".Replace(" ", string.Empty),
                    n => n != 1 ? 1 : 0
                },
                {
                    "plural=(n==0 ? 0 : n==1 ? 1 : n==2 ? 2 : n%100>=3 && n%100<=10 ? 3 : n%100>=11 ? 4 : 5)".Replace(" ", string.Empty),
                    n => n == 0 ? 0 : n == 1 ? 1 : n == 2 ? 2 : n % 100 >= 3 && n % 100 <= 10 ? 3 : n % 100 >= 11 ? 4 : 5
                },
                {
                    "plural=0".Replace(" ", string.Empty),
                    n => 0
                },
                {
                    "plural=(n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)".Replace(" ", string.Empty),
                    n => n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2
                },
                {
                    "plural=(n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2".Replace(" ", string.Empty),
                    n => n == 1 ? 0 : (n >= 2 && n <= 4) ? 1 : 2
                },
                {
                    "plural=(n==1) ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2".Replace(" ", string.Empty),
                    n => (n == 1) ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2
                },
                {
                    "plural=(n==1) ? 0 : (n==2) ? 1 : (n != 8 && n != 11) ? 2 : 3".Replace(" ", string.Empty),
                    n => (n == 1) ? 0 : (n == 2) ? 1 : (n != 8 && n != 11) ? 2 : 3
                },
                {
                    "plural=n==1 ? 0 : n==2 ? 1 : (n>2 && n<7) ? 2 :(n>6 && n<11) ? 3 : 4".Replace(" ", string.Empty),
                    n => n == 1 ? 0 : n == 2 ? 1 : (n > 2 && n < 7) ? 2 : (n > 6 && n < 11) ? 3 : 4
                },
                {
                    "plural=(n==1 || n==11) ? 0 : (n==2 || n==12) ? 1 : (n > 2 && n < 20) ? 2 : 3".Replace(" ", string.Empty),
                    n => (n == 1 || n == 11) ? 0 : (n == 2 || n == 12) ? 1 : (n > 2 && n < 20) ? 2 : 3
                },
                {
                    "plural=(n%10!=1 || n%100==11)".Replace(" ", string.Empty),
                    n => (n % 10 != 1 || n % 100 == 11) ? 1 : 0
                },
                {
                    "plural=(n != 0)".Replace(" ", string.Empty),
                    n => n != 0 ? 1 : 0
                },
                {
                    "plural=(n==1) ? 0 : (n==2) ? 1 : (n == 3) ? 2 : 3".Replace(" ", string.Empty),
                    n => (n == 1) ? 0 : (n == 2) ? 1 : (n == 3) ? 2 : 3
                },
                {
                    "plural=(n%10==1 && n%100!=11 ? 0 : n%10>=2 && (n%100<10 || n%100>=20) ? 1 : 2)".Replace(" ", string.Empty),
                    n => n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2
                },
                {
                    "plural=(n%10==1 && n%100!=11 ? 0 : n != 0 ? 1 : 2)".Replace(" ", string.Empty),
                    n => n % 10 == 1 && n % 100 != 11 ? 0 : n != 0 ? 1 : 2
                },
                {
                    "plural=n==1 || n%10==1 ? 0 : 1".Replace(" ", string.Empty), //Can’t be correct needs a 2 somewhere
                    n => n == 1 || n % 10 == 1 ? 0 : 1
                },
                {
                    "plural=(n==0 ? 0 : n==1 ? 1 : 2)".Replace(" ", string.Empty),
                    n => n == 0 ? 0 : n == 1 ? 1 : 2
                },
                {
                    "plural=(n==1 ? 0 : n==0 || ( n%100>1 && n%100<11) ? 1 : (n%100>10 && n%100<20 ) ? 2 : 3)".Replace(" ", string.Empty),
                    n => n == 1 ? 0 : n == 0 || (n % 100 > 1 && n % 100 < 11) ? 1 : (n % 100 > 10 && n % 100 < 20) ? 2 : 3
                },
                {
                    "plural=(n==1 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)".Replace(" ", string.Empty),
                    n => n == 1 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2
                },
                {
                    "plural=(n==1 ? 0 : (n==0 || (n%100 > 0 && n%100 < 20)) ? 1 : 2)".Replace(" ", string.Empty),
                    n => n == 1 ? 0 : (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? 1 : 2
                },
                {
                    "plural=(n%100==1 ? 1 : n%100==2 ? 2 : n%100==3 || n%100==4 ? 3 : 0)".Replace(" ", string.Empty),
                    n => n % 100 == 1 ? 1 : n % 100 == 2 ? 2 : n % 100 == 3 || n % 100 == 4 ? 3 : 0
                }
            };
        }

        public Func<int, int> GetEvaluatingMethod(string expression)
        {
            var trimmedExpression = expression.Replace(" ", string.Empty);
            if (_mappedMethods.ContainsKey(trimmedExpression))
                return _mappedMethods[trimmedExpression];

            Debug.LogWarning("Unable to parse expression: " + expression);
            return n => 0;
        }
    }
}