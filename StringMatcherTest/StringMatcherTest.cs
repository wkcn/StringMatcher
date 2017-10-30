using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StringMatcherNS;
using System.Text;

namespace StringMatcherTest
{
    [TestClass]
    public class StringMatcherTest
    {
        public const int TIMEOUT_MS = 5000; // 5s
        [TestMethod]
        [Timeout(TIMEOUT_MS)]
        // 简单测试
        public void TestSimpleString()
        {
            Assert.AreEqual(StringMatcher.Match("", ""), true);
            Assert.AreEqual(StringMatcher.Match("", "a"), false);
            Assert.AreEqual(StringMatcher.Match("a", ""), false);
            Assert.AreEqual(StringMatcher.Match("abc", "abc"), true);
            Assert.AreEqual(StringMatcher.Match("adc", "abc"), false);
        }

        [TestMethod]
        [Timeout(TIMEOUT_MS)]
        public void TestStar()
        {
            Assert.AreEqual(StringMatcher.Match("*", ""), true);
            Assert.AreEqual(StringMatcher.Match("a*b", "ab"), true);
            Assert.AreEqual(StringMatcher.Match("a*b", "awb"), true);
            Assert.AreEqual(StringMatcher.Match("a*b", "awkb"), true);
            Assert.AreEqual(StringMatcher.Match("a*b", "cawkb"), false);
            Assert.AreEqual(StringMatcher.Match("a*b", "awkbc"), false);
        }

        [TestMethod]
        [Timeout(TIMEOUT_MS)]
        public void TestAdd()
        {
            Assert.AreEqual(StringMatcher.Match("+", ""), false);
            Assert.AreEqual(StringMatcher.Match("a+b", "ab"), false);
            Assert.AreEqual(StringMatcher.Match("a+b", "awb"), true);
            Assert.AreEqual(StringMatcher.Match("a+b", "awkb"), true);
            Assert.AreEqual(StringMatcher.Match("a+b", "cawkb"), false);
            Assert.AreEqual(StringMatcher.Match("a*b", "awkbc"), false);
        }

        [TestMethod]
        [Timeout(TIMEOUT_MS)]
        public void TestQuesMask()
        {
            Assert.AreEqual(StringMatcher.Match("?", ""), false);
            Assert.AreEqual(StringMatcher.Match("a?b", "ab"), false);
            Assert.AreEqual(StringMatcher.Match("a?b", "awb"), true);
            Assert.AreEqual(StringMatcher.Match("a?b", "awkb"), false);
            Assert.AreEqual(StringMatcher.Match("a?b", "cawkb"), false);
            Assert.AreEqual(StringMatcher.Match("a?b", "awkbc"), false);
        }

        [TestMethod]
        [Timeout(TIMEOUT_MS)]
        public void TestEscape()
        {
            Assert.AreEqual(StringMatcher.Match(@"a\\b", @"a\b"), true);
            Assert.AreEqual(StringMatcher.Match(@"a\*b", @"a*b"), true);
            Assert.AreEqual(StringMatcher.Match(@"a\+b", @"a+b"), true);
            Assert.AreEqual(StringMatcher.Match(@"a\?b", @"a?b"), true);
            Assert.AreEqual(StringMatcher.Match(@"a\\?b", @"a\eb"), true);

            Assert.AreEqual(StringMatcher.Match(@"a\\b", @"a*b"), false);
            Assert.AreEqual(StringMatcher.Match(@"a\*b", @"a+b"), false);
            Assert.AreEqual(StringMatcher.Match(@"a\+b", @"a?b"), false);
            Assert.AreEqual(StringMatcher.Match(@"a\?b", @"a\b"), false);
            Assert.AreEqual(StringMatcher.Match(@"a\\?b", @"a\ec"), false);

            // 输入异常检查
            bool throwed = false;
            try
            {
                // 转义符号后面没有字符
                StringMatcher.Match(@"\", @"\");
            }
            catch (FormatException e)
            {
                Assert.AreEqual(e.Message, StringMatcher.ESCAPE_CHARACTER_ERROR);
                throwed = true;
            }

            if (!throwed)
                Assert.Fail("No exception was thrown.");

            throwed = false;
            try
            {
                // 转义符号后面的字符不是*,+,?,\
                StringMatcher.Match(@"\a", @"\");
            }
            catch (FormatException e)
            {
                Assert.AreEqual(e.Message, StringMatcher.ESCAPE_CHARACTER_ERROR);
                throwed = true;
            }

            if (!throwed)
                Assert.Fail("No exception was thrown.");  

        }

        [TestMethod]
        [Timeout(TIMEOUT_MS)]
        public void TestSpecial()
        {
            Assert.AreEqual(StringMatcher.Match(@"*?Mc\\l", @"ctUVUdLMc\l"), true);
        }

        // 测试样本指标(近似值)：
        const int STR_MAX_LENGTH = 20; // 字符串长度： 0 ~ STR_MAX_LENGTH
        const int NUM_TRUE_SAMPLE = 1000; // 匹配的样例
        const int NUM_FALSE_SAMPLE = 1000; // 不匹配的样例
        const int NUM_STAR_SAMPLE = 500; // *
        const int NUM_ADD_SAMPLE = 500; // +
        const int NUM_QUES_SAMPLE = 500; // ?
        const int NUM_ESCAPE_RIGHT = 200; // 正确的转义
        const int NUM_ESCAPE_WRONG = 10; // 错误的转义
        const int NUM_GOOD_SAMPLE = 500;

        enum RECORD_IDX
        {
            NULL, TRUE, FALSE, STAR, ADD, QUES, ESCAPE_RIGHT, ESCAPE_WRONG, GOOD
        }

        static Random random = new Random(3939);
        static string wildcards = @"*+?\";

        private char get_random_alpha()
        {
            if (random.Next(0, 2) == 0)
                return (char)random.Next('a', 'z' + 1);
            return (char)random.Next('A', 'Z' + 1);
        }

        private int add_random_string(ref string value)
        {
            int w = random.Next(0, 5);
            for (int u = 0; u < w; ++u)
            {
                value += get_random_alpha();
            }
            return w;
        }

        private int add_special_string(ref string value, ref string buffer, ref bool true_sample)
        {
            if (buffer.Length == 0) return 0;
            int num_add = 0;
            int num_ques = 0;
            char c = buffer[0];
            if (c == '+') ++num_add;
            if (c == '?') ++num_ques;
            for (int i = 1; i < buffer.Length; ++i)
            {
                char p = buffer[i];
                if (c == '+')
                {
                    if (p == '+') ++num_add;
                    else if (p == '?') ++num_add;
                    else if (p == '*') { };
                }
                else if (c == '*')
                {
                    if (p == '+') { ++num_add; c = '+'; }
                    else if (p == '?') { ++num_add; c = '+'; }
                    else if (p == '*') { };
                }
                else if (c == '?')
                {
                    if (p == '+') { num_add += 1 + num_ques; c = '+'; num_ques = 0; }
                    else if (p == '?') ++num_ques;
                    else if (p == '*') { num_add += num_ques; c = '+'; num_ques = 0; };
                }
            }

            bool hasmore = false;
            int lower = 0;
            if (c == '+')
            {
                hasmore = true;
                lower = num_add;
            }
            else if (c == '*')
            {
                hasmore = true;
            }
            else if (c == '?')
            {
                lower = num_ques;
            }

            int delta = random.Next(lower > 2 ? -lower : 2, 3);

            delta = 0; // 还需要完善

            int siz = delta + lower;
            for (int i = 0; i < siz; ++i)
            {
                value += get_random_alpha();
            }
            if (delta < 0)
            {
                true_sample = false;
            }
            if (delta > 0 && !hasmore)
            {
                true_sample = false;
            }
            // Console.Write("{0},{1},{2},{3},{4},{5} - ", c, num_add, num_ques,siz,buffer,true_sample);
            buffer = "";

            return siz;
        }

        private void RandomTestOnce(int[] record) 
        {
            int len = random.Next(0, 20);
            string pattern = "";
            string value = "";
            bool true_sample = true;
            int false_i = -1;
            bool escape_wrong = false;
            bool star_sample = false;
            bool add_sample = false;
            bool ques_sample = false;
            bool escape_sample = false;

            string buffer = ""; // 用于记录特殊符号*+?
            for (int i = 0; i < len; ++i)
            {
                if (!true_sample && false_i == -1)
                {
                    false_i = i;
                    break;
                }

                int r = random.Next(0, 100);
                if (0 <= r && r < 10)
                {
                    // *
                    pattern += '*';
                    buffer += '*';
                    star_sample = true;
                }
                else if (10 <= r && r < 20)
                {
                    // +
                    pattern += '+';
                    buffer += '+';
                    add_sample = true;
                }
                else if (20 <= r && r < 30)
                {
                    // ?
                    pattern += '?';
                    buffer += '?';
                    ques_sample = true;
                }
                else if (r >= 30)
                {
                    add_special_string(ref value, ref buffer, ref true_sample);
                    if (30 <= r && r < 40)
                    {
                        // escape
                        // 转义符号测试
                        escape_sample = true;
                        pattern += '\\';
                        char wildcard = wildcards[random.Next(0, wildcards.Length)];
                        if (random.Next(0, 10) != 4)
                        {
                            // 生成pattern合法的转义符测试样例
                            pattern += wildcard;
                            if (random.Next(0, 10) < 8)
                            {
                                // 生成value与pattern对应元素匹配的样例
                                value += wildcard;
                            }
                            else
                            {
                                // 生成转义符被忽略的样例, value错误
                                add_random_string(ref value);
                                true_sample = false;
                            }
                        }
                        else
                        {
                            // 生成错误的转义符样例，pattern错误
                            pattern += 'a'; // exception test
                            value += 'a';
                            escape_wrong = true;
                        }
                    }
                    else
                    {
                        char c = get_random_alpha();
                        pattern += c;
                        if (random.Next(0, 10) < 7){
                            // 生成正确的样例 
                            value += c;
                        }
                        else if (random.Next(0, 30) > 28)
                        {
                            // value比pattern少一个字符
                            true_sample = false;
                        }
                        else{
                            char c2 = get_random_alpha();
                            if (c2 != c)
                            {
                                // value和pattern存在至少一个字符不匹配
                                true_sample = false;
                            }
                            value += c2;
                        }
                    }
                }
            }
            add_special_string(ref value, ref buffer, ref true_sample);

            if (escape_wrong) true_sample = false;

            try
            {
                Console.WriteLine("{0}, {1}, [ANSWER]: {2}, exception: {3}, false_i: {4}", pattern, value, true_sample, escape_wrong,false_i);
                Assert.AreEqual(StringMatcher.Match(pattern, value), true_sample);
                Assert.AreEqual(escape_wrong, false);
            }
            catch (FormatException e)
            {
                Assert.AreEqual(escape_wrong, true);
                Assert.AreEqual(e.Message, StringMatcher.ESCAPE_CHARACTER_ERROR);
            }

            // 记录测试样例类型
            if (true_sample) ++record[(int)RECORD_IDX.TRUE];
            else ++record[(int)RECORD_IDX.FALSE];

            if (star_sample) ++record[(int)RECORD_IDX.STAR];
            if (add_sample) ++record[(int)RECORD_IDX.ADD];
            if (ques_sample) ++record[(int)RECORD_IDX.QUES];

            if (true_sample && len > 5)
            {
                ++record[(int)RECORD_IDX.GOOD];
            }
            if (escape_sample)
            {
                if (escape_wrong) ++record[(int)RECORD_IDX.ESCAPE_WRONG];
                else ++record[(int)RECORD_IDX.ESCAPE_RIGHT];
            }
        }

        [TestMethod]
        [Timeout(TIMEOUT_MS)]
        // 随机测试
        public void TestRandomString()
        {

            int[] record = new int[9];
            while(
                record[(int)RECORD_IDX.TRUE] < NUM_TRUE_SAMPLE ||
                record[(int)RECORD_IDX.FALSE] < NUM_FALSE_SAMPLE ||
                record[(int)RECORD_IDX.STAR] < NUM_STAR_SAMPLE ||
                record[(int)RECORD_IDX.ADD] < NUM_ADD_SAMPLE ||
                record[(int)RECORD_IDX.QUES] < NUM_QUES_SAMPLE ||
                record[(int)RECORD_IDX.ESCAPE_RIGHT] < NUM_ESCAPE_RIGHT ||
                record[(int)RECORD_IDX.ESCAPE_WRONG] < NUM_ESCAPE_WRONG ||
                record[(int)RECORD_IDX.GOOD] < NUM_GOOD_SAMPLE
                )
            {
                RandomTestOnce(record);
            }

            Console.WriteLine("Test OK, TRUE_SAMPLE:{0}, FALSE_SAMPLE:{1}, STAR_SAMPLE:{2}, ADD_SAMPLE:{3}, QUES_SAMPLE:{4}, ESCAPE_RIGHT:{5}, ESCAPE_WRONG:{6}, GOOD_SAMPLE:{7}",
                record[(int)RECORD_IDX.TRUE],
                record[(int)RECORD_IDX.FALSE],
                record[(int)RECORD_IDX.STAR],
                record[(int)RECORD_IDX.ADD],
                record[(int)RECORD_IDX.QUES],
                record[(int)RECORD_IDX.ESCAPE_RIGHT],
                record[(int)RECORD_IDX.ESCAPE_WRONG],
                record[(int)RECORD_IDX.GOOD]);

        }

    }
}
