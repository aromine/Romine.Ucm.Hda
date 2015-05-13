using System;
using System.Collections.Generic;
using System.Text;

namespace Romine.Ucm.Hda
{
    public static class HdaStringUtils
    {
        public static string Encode(string input)
        {
            //return input;
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            StringBuilder output = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                switch (c)
                {
                    case '\\':
                    case '@':
                        output.Append('\\');
                        break;
                    case '\n':
                        output.Append(@"\n");
                        continue;
                    case '\r':
                        output.Append(@"\r");
                        continue;
                }

                output.Append(c);
            }

            return output.ToString();
        }

        public static string Decode(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            if (input.IndexOf('\\') < 0)
            {
                return input;
            }

            StringBuilder output = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (i < input.Length)
                {
                    if (c == '\\')
                    {
                        c = input[++i]; //read the next char

                        switch (c)
                        {
                            case 'n': //Newline or return carriage append the correct item.
                                c = '\n';
                                break;
                            case 'r':
                                c = '\r';
                                break;
                            //case '\\': //TODO are these all we need to worry about?
                            //    c = '\\'; //this is also the current value of c!
                            //    break;
                            //default:
                            //    output.Append(c); //This is the case that we have just a plane old escaped '@'
                            //    break;
                            default:

                                break;
                        }
                    }
                }

                output.Append(c);
            }

            return output.ToString();
        }
    }
}
