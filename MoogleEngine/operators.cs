namespace MoogleEngine
{
    public class operators
    {
        // operators '!' and '^' return a tuple (bool, string[]) (presence of operator or not, word affected by operator)
        // operator '*' returns (presence of operator or not, word affected by operator, number of '*' in query)
        public static (bool operatorPresence, string[] affectedWords) nonPresent(string query)
        {
            return operatorAction('!', query);
        }

        public static (bool operatorPresence, string[] affectedWords) Present(string query)
        {
            return operatorAction('^', query);
        }

        public static (bool, string[], int numberOfOperator) Importance(string query)
        {
            (bool, string[], int) Importance = (false, new string[0], 0);
            (bool, string[]) affects = operatorAction('*', query);
            Importance.Item1 = affects.Item1;
            Importance.Item2 = affects.Item2;
            // So far normal operator's behaivor

            // Counting number of * in query
            query = query.Trim();
            int asteriskCount = 0;
            for (int c = 0; c < query.Length; c++)
            {
                if (query[c] == '*')
                {
                    asteriskCount++;
                }
            }
            // Returning total of '*' in query
            Importance.Item3 = asteriskCount;

            return Importance;
        }

        public static (bool, Dictionary<string, string[]>) closeness(string query)
        {
            (bool, Dictionary<string, string[]>) closeness = (false, new Dictionary<string, string[]>());
            if (query.Contains('~'))
            {
                closeness.Item1 = true;
            }
            
            query = query.Trim();

            for (int i = 0; i < query.Length; i++)
            {
                if (query[i] == '~')
                {
                    // closeness operator affected 2 words each time it appears
                    string w1 = "";
                    string w2 = "";
                    int end = 0;
                    int start = 0;

                    for (int c = i-1; c >= 0; c--)
                    {
                        // First word from pair affected by operator
                        if (query[c] != ' ') 
                        {
                            for(int j = c; j >= 0; j--)
                            {
                                if (query[j] == ' ' || j == 0)
                                {
                                    start = j;
                                    end = c;
                                    w1 = query.Substring(start, (end - start) + 1); // From c to operator first word of pair
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    for (int c = i+1; c < query.Length; c++)
                    {
                        // Second word from pair
                        if (query[c] != ' ') 
                        {
                            for(int j = c; j < query.Length; j++)
                            {
                                if (query[j] == ' ' || j == query.Length - 1)
                                {
                                    start = c;
                                    end = j;
                                    w2 = query.Substring(start, (end - start) + 1); // From c to operator first word of pair
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    w2 = w2.ToLower().Trim();
                    w1 = w1.ToLower().Trim();

                    // Updating dictionary of affected words
                    if (closeness.Item2.ContainsKey(w1))
                    {
                        closeness.Item2[w1].Append(w2);
                    }
                    else
                    {
                        string[] value = new string[1];
                        value[0] = w2;
                        closeness.Item2.Add(w1,value);
                    }

                    if (closeness.Item2.ContainsKey(w2))
                    {
                        closeness.Item2[w2].Append(w1);
                    }
                    else
                    {
                        string[] value = new string[1];
                        value[0] = w1;
                        closeness.Item2.Add(w2,value);
                    }
                }
            }

            return closeness;
        }

        // Aux method
        public static (bool, string[]) operatorAction(char oper, string query)
        {
            // Preparing answers
            List<string> words = new List<string>();
            (bool, string[]) answer = (false, new string[0]);

            query = query.Trim();// Preparing query

            if (query.Contains(oper))
            {   
                // Operator is in query
                answer.Item1 = true;
                
                // Iterating over query looking for operators and words it affects
                for (int c = 0; c < query.Length; c++)
                {
                    if(query[c] == oper)
                    {
                        for (int s = c; s < query.Length; s++)
                        {
                            if(query[s] == ' ' || s == query.Length-1)
                            {
                                string word = query.Substring(c+1, s-c); // Affected word starts after operators and ends on next white space or end of query 
                                word = word.Trim();
                                words.Add(word); // Agregate word to the array would be returned
                                break; // Go out loop and look for new operator and new word
                            }
                        }
                    }
                }
                // returning affected word/s
                answer.Item2 = words.ToArray();
            }

            return answer;
        }
    }

    
}