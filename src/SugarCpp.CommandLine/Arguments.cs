using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.CommandLine
{
    class Arguments
    {
        internal Arguments(string[] args, Dictionary<string, bool> options)
        {
            this.args = args;
            this.Options = new Dictionary<string,string>();
            this.DirectArguments = new List<string>();
            if (options == null)
            {
                options = new Dictionary<string, bool>();
            }
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                string nextArg = null;
                if (i < args.Length - 1)
                {
                    nextArg = args[i + 1];
                }

                if (arg.Length > 0)
                {
                    string opt = null;
                    string optArg = null;
                    if (arg.StartsWith("-"))
                    {
                        if (arg.StartsWith("--"))
                        {
                            // --option
                            int equalSign = arg.IndexOf('=');
                            if (equalSign != -1)
                            {
                                // --option=value
                                opt = arg.Substring(2, equalSign - 2);
                                optArg = arg.Substring(equalSign + 1);
                            }
                            else
                            {
                                // --option value
                                opt = arg.Substring(2);
                            }
                        }
                        else
                        {
                            // -o
                            opt = arg.Substring(1);
                        }
                    }
                    else if (arg.StartsWith("/"))
                    {
                        // /option
                        int comma = arg.IndexOf(':');
                        if (comma != -1)
                        {
                            // /option:value
                            opt = arg.Substring(1, comma - 1);
                            optArg = arg.Substring(comma + 1);
                        }
                        else
                        {
                            // /option value
                            opt = arg.Substring(1);
                        }
                    }

                    if (opt != null && options.ContainsKey(opt))
                    {
                        bool hasArg = options[opt];
                        if (hasArg)
                        {
                            if (optArg == null)
                            {
                                if (nextArg == null)
                                {
                                    Program.Panic("No argument after " + opt);
                                }
                                optArg = nextArg;
                                i++;
                            }
                        }
                        else
                        {
                            if (optArg != null)
                            {
                                Program.Panic("No argument should be after " + opt);
                            }
                        }
                        this.Options[opt] = optArg;
                    }
                    else
                    {
                        this.DirectArguments.Add(arg);
                    }
                }
            }
        }

        public bool HasOption(string option)
        {
            return this.Options.ContainsKey(option);
        }

        public string GetOption(string option)
        {
            if (this.HasOption(option))
            {
                return this.Options[option];
            }
            return null;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                buffer.Append('"');
                buffer.Append(args[i]);
                buffer.Append('"');
                if (i != args.Length - 1)
                {
                    buffer.Append(' ');
                }
            }
            return buffer.ToString();
        }

        public void Print()
        {
            foreach (var item in this.DirectArguments)
            {
                Console.WriteLine(item);
            }
            foreach (var item in this.Options)
            {
                Console.WriteLine(item.Key + ":" + item.Value);
            }
        }

        public List<string> DirectArguments { get; private set; }

        public Dictionary<string, string> Options { get; private set; }

        private string[] args;
    }
}
