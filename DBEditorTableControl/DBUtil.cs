using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DBTableControl
{
    public static class DBUtil
    {
        public static bool isMatch(string input, string pattern, MatchType option = MatchType.Simple)
        {
            // Simple text match path.
            if (option == MatchType.Simple)
            {
                if (input.Equals(pattern))
                {
                    return true;
                }
            }

            // Wildcard match path.
            if (option == MatchType.Wildcard)
            {

            }

            // Regex match path.
            if (option == MatchType.Regex)
            {
                Regex matchregex = new Regex(pattern);
                return matchregex.IsMatch(input);
            }

            return false;
        }

        public enum MatchType
        {
            Simple,
            Wildcard,
            Regex
        }
    }

    // Custom comparer for the table to sort columns correctly based on supposed data type.
    public class ColumnComparer : IComparer
    {
        private ListSortDirection sortdirection;
        private ColumnType sorttype;

        public ColumnComparer(ListSortDirection direction, ColumnType coltype)
        {
            sortdirection = direction;
            sorttype = coltype;
        }

        // Compare method.
        public int Compare(object x, object y)
        {
            int sortmultiplier = (sortdirection == ListSortDirection.Ascending) ? 1 : -1;
            if (sorttype == ColumnType.Number)
            {
                return CompareNumberColumn(x, y) * sortmultiplier;
            }
            else if (sorttype == ColumnType.Text)
            {
                return CompareTextColumn(x, y) * sortmultiplier;
            }
            else
            {
                return 0;
            }
        }

        private int CompareTextColumn(object x, object y)
        {
            string testx = x.ToString();
            string testy = y.ToString();

            return String.Compare(testx, testy);
        }

        private int CompareNumberColumn(object x, object y)
        {
            if ((double)x < (double)y)
            {
                return -1;
            }
            else if ((double)y > (double)y)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public enum ColumnType
    {
        Text,
        Number
    }
}
