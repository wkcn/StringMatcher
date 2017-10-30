using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringMatcherNS
{
    public class StringMatcher
    {
        public const string ESCAPE_CHARACTER_ERROR = @"转义字符\后面必须存在字符，并且该字符为*,+,?,\之一";
        /*
         * pattern: 包含通配符的模式串
         * value:   待匹配的字符串
         * Return:  待匹配的字符串是否和模式串匹配
         * 条件：
         * (a) 通配符包扩 *, +, ?, 其中，* 匹配任意多个任意字符（可以为空），+匹配1个或以上任意字符，?匹配一个任意字符
         * (b) 支持转义符号 ‘\’，\*、  \+、 \? 、\\ 分别匹配特殊字符 *, + ?, \
           Exception: 这里定义，如果转移字符后不存在字符或者后面的字符不是*, + ?, \, 则抛出FormatException异常
         *            \n, \t也算非法的模式串
         */
        public static bool Match(string pattern, string value)
        {
            // 首先检查是否存在\a等错误转义
            CheckPatternFormat(pattern);
            return MatchIn(pattern, value, 0, 0);
        }

        // 检查pattern是否合法（转义符是否正确）
        private static void CheckPatternFormat(string pattern)
        {
            int i = 0;
            while (i < pattern.Length)
            {
                if (pattern[i] == '\\')
                {
                    ++i; // 转义符号的下一个字符
                    if (i >= pattern.Length) throw new FormatException(ESCAPE_CHARACTER_ERROR);
                    char fc = pattern[i];
                    if (fc != '\\' && fc != '*' && fc != '+' && fc != '?') throw new FormatException(ESCAPE_CHARACTER_ERROR);
                }
                ++i;
            }
        }

        /*
         * i: index of pattern
         * j: index of value
         */
        private static bool MatchIn(string pattern, string value, int i, int j)
        {
            if (i < pattern.Length)
            {

                char pc = pattern[i];
                // 当'*'是pattern的最后一个字符，且j >= value.Length时，字符串匹配
                // 因此，将pc == '*'的判断放在条件j < value.Length外
                if (pc == '*')
                {
                    for (int u = j; u <= value.Length; ++u)
                    {
                        if (MatchIn(pattern, value, i + 1, u)) return true;
                    }
                }

                if (j < value.Length)
                {
                    if (pc == '+')
                    {
                        for (int u = j + 1; u <= value.Length; ++u)
                        {
                            if (MatchIn(pattern, value, i + 1, u)) return true;
                        }
                    }
                    else if (pc == '?')
                    {
                        return MatchIn(pattern, value, i + 1, j + 1);
                    }
                    else if (pc == '\\')
                    {
                        // 转义字符
                        // 这里定义，如果转移字符后不存在字符或者后面的字符不是*, + ?, \, 则抛出FormatException异常
                        if (i + 1 >= pattern.Length) throw new FormatException(ESCAPE_CHARACTER_ERROR);
                        char fc = pattern[i + 1];
                        if (fc == '*' || fc == '+' || fc == '?' || fc == '\\')
                        {
                            if (value[j] == fc) return MatchIn(pattern, value, i + 2, j + 1);
                            else return false;
                        }
                        else
                        {
                            throw new FormatException(ESCAPE_CHARACTER_ERROR);
                        }
                    }
                    else
                    {
                        // 普通字符
                        if (pc == value[j]) return MatchIn(pattern, value, i + 1, j + 1);
                        else return false;
                    }
                }
            }
            return (i >= pattern.Length && j >= value.Length);
        }
    }
}
