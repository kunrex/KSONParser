using System;
using System.Reflection;
using System.Collections.Generic;

namespace ANFChallenge
{
    public class KsonParser
    {
        public static object FromKson<T>(string ksonString)
        {
            Console.WriteLine("Converting to class instance\n");

            Type type = typeof(T);

            var t = GetInstance(type);

            bool varNameFound = false;
            string varName = string.Empty;
            string value = string.Empty;

            for(int i =0;i<ksonString.Length;i++)
            {
                switch (ksonString[i])
                {
                    case char k when k == '{' || k == '}':
                        break;
                    case '"':
                        varNameFound = !varNameFound;
                        if (varNameFound)
                            varName = string.Empty;
                        break;
                    case ';':

                        FieldInfo field = t.GetType().GetField(varName);

                        if (value == "NULL")
                        {
                            field.SetValue(t, null);
                        }
                        else
                        {
                            Type _type = field.FieldType;
                            int integer; float floatingPoint;

                            if (field.FieldType.IsArray)
                            {
                                string currentValue = string.Empty;
                                List<string> listS = new List<string>(); List<int> listI = new List<int>(); List<float> listF = new List<float>(); List<bool> listB = new List<bool>();

                                for (int k = 0; k < value.Length; k++)
                                {
                                    switch (value[k])
                                    {
                                        case '[':
                                        case ' ':
                                            break;

                                        case char _charecter when char.IsLetter(_charecter) || char.IsNumber(_charecter) || _charecter == '.':
                                            currentValue += value[k];
                                            break;
                                        case char _charecter when _charecter == ',' || _charecter == ']':

                                            switch (field.FieldType)
                                            {
                                                case Type TYPE when TYPE == typeof(string[]):
                                                    listS.Add(currentValue);
                                                    break;
                                                case Type TYPE when TYPE == typeof(int[]):
                                                    listI.Add(int.Parse(currentValue));
                                                    break;
                                                case Type TYPE when TYPE == typeof(float[]):
                                                    listF.Add(float.Parse(currentValue));
                                                    break;
                                                case Type TYPE when TYPE == typeof(bool[]):
                                                    listB.Add(currentValue == "TRUE");
                                                    break;
                                            }

                                            currentValue = string.Empty;
                                            break;
                                    }

                                    switch (field.FieldType)
                                    {
                                        case Type TYPE when TYPE == typeof(string[]):
                                            field.SetValue(t, listS.ToArray());
                                            break;
                                        case Type TYPE when TYPE == typeof(int[]):
                                            field.SetValue(t, listI.ToArray());
                                            break;
                                        case Type TYPE when TYPE == typeof(float[]):
                                            field.SetValue(t, listF.ToArray());
                                            break;
                                        case Type TYPE when TYPE == typeof(bool[]):
                                            field.SetValue(t, listB.ToArray());
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                if (_type == typeof(int))
                                {
                                    integer = int.Parse(value);
                                    field.SetValue(t, integer);
                                }
                                else if (_type == typeof(float))
                                {
                                    floatingPoint = float.Parse(value);
                                    field.SetValue(t, floatingPoint);
                                }
                                else if (_type == typeof(string))
                                {
                                    field.SetValue(t, value);
                                }
                                else if (_type == typeof(bool))
                                {
                                    field.SetValue(t, value != "TRUE");
                                }
                            }
                        }

                        value = string.Empty;
                        break;
                    case char _char when _char.IsViableToBeParsed():
                        if (varNameFound)
                            varName += _char;
                        if (!varNameFound)
                            value += _char;
                        break;
                }
            }

            Console.WriteLine("Done\n");
            return t;

            //the first use of a local method
            object GetInstance(Type type)
            {
                return Activator.CreateInstance(type);
            }
        }

        public static string ToKson(object instance)
        {
            Console.WriteLine("Converting to Kson text\n");

            FieldInfo[] fieldValues = instance.GetType().GetFields();

            string parsed = "{\n";
            for(int i = 0;i < fieldValues.Length;i++)
            {
                if (fieldValues[i].GetValue(instance) == null)
                {
                    parsed += $"   \"{fieldValues[i].Name}\":NULL;\n";
                    continue;
                }

                if (!fieldValues[i].FieldType.IsArray)
                {
                    
                    if (fieldValues[i].FieldType == typeof(bool))
                        parsed += $"   \"{fieldValues[i].Name}\":{((bool)fieldValues[i].GetValue(instance)==true).ToString().ToUpper()};\n"; 
                    else
                        parsed += $"   \"{fieldValues[i].Name}\":{fieldValues[i].GetValue(instance)};\n";
                }
                else
                {
                    if(fieldValues[i].FieldType == typeof(int[]))
                    {
                        int[] arr = (int[])fieldValues[i].GetValue(instance);
                        string values = "[";

                        for(int k =0;k<arr.Length;k++)
                        {
                            if(k + 1 != arr.Length)
                                values += $"{arr[k]},";
                            else
                                values += $"{arr[k]}";
                        }
                        values += "]";

                        parsed += $"\n   \"{fieldValues[i].Name}\":{values};\n";
                    }
                    else if(fieldValues[i].FieldType == typeof(string[]))
                    {
                        string[] arr = (string[])fieldValues[i].GetValue(instance);
                        string values = "[";

                        for (int k = 0; k < arr.Length; k++)
                        {
                            if (k + 1 != arr.Length)
                                values += $"'{arr[k]}',";
                            else
                                values += $"'{arr[k]}'";
                        }
                        values += "]";

                        parsed += $"\n   \"{fieldValues[i].Name}\":{values};\n";
                    }
                    else if (fieldValues[i].FieldType == typeof(float[]))
                    {
                        float[] arr = (float[])fieldValues[i].GetValue(instance);
                        string values = "[";

                        for (int k = 0; k < arr.Length; k++)
                        {
                            if (k + 1 != arr.Length)
                                values += $"{arr[k]},";
                            else
                                values += $"{arr[k]}";
                        }
                        values += "]";

                        parsed += $"\n   \"{fieldValues[i].Name}\":{values};\n";
                    }
                    else if (fieldValues[i].FieldType == typeof(bool[]))
                    {
                        bool[] arr = (bool[])fieldValues[i].GetValue(instance);
                        string values = "[";

                        for (int k = 0; k < arr.Length; k++)
                        {
                            if (k + 1 != arr.Length)
                                values += $"{arr[k].ToString().ToUpper()},";
                            else
                                values += $"{arr[k].ToString().ToUpper()}";
                        }
                        values += "]";

                        parsed += $"\n   \"{fieldValues[i].Name}\":{values};\n";
                    }
                }
            }
            parsed += "}\n";

            return parsed;
        }
    }
}
