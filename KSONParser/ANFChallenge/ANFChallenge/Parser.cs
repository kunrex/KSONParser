using System;
using System.Reflection;
using System.Collections.Generic;
using HelpfulExtensions;
using System.Collections;

namespace ANFChallenge
{
    public class KsonParser
    {
        const string NULL = "NULL";
        const string SPACE = "   ";

        public static object FromKson<T>(string ksonString)
        {
            Console.WriteLine("Converting to class instance\n");

            Type type = typeof(T);

            var instance = Activator.CreateInstance(type);

            bool varNameFound = false;
            string varName = string.Empty, value = string.Empty;

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
                        FieldInfo field = instance.GetType().GetField(varName);

                        if (value == NULL)
                        {
                            field.SetValue(instance, null);
                        }
                        else
                        {
                            Type fieldType = field.FieldType;

                            if (field.FieldType.IsArray)
                            {
                                Type listType = typeof(List<>).MakeGenericType(fieldType.GetElementType());
                                dynamic list = Activator.CreateInstance(listType);

                                string currentValue = string.Empty;

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
                                                case Type dataType when dataType == typeof(string[]):
                                                    list.Add(currentValue);
                                                    break;
                                                case Type dataType when dataType == typeof(int[]):
                                                    list.Add(int.Parse(currentValue));
                                                    break;
                                                case Type dataType when dataType == typeof(float[]):
                                                    list.Add(float.Parse(currentValue));
                                                    break;
                                                case Type dataType when dataType == typeof(bool[]):
                                                    list.Add(currentValue == "TRUE");
                                                    break;
                                                case Type dataType when dataType == typeof(bool[]):
                                                    list.Add(currentValue.ToChar());
                                                    break;
                                                default:
                                                    throw new Exception("Invalid Type To Deserialise");
                                            }

                                            currentValue = string.Empty;
                                            break;
                                    }

                                    field.SetValue(instance, list.ToArray());
                                }
                            }
                            else
                            {
                                if (fieldType == typeof(int))
                                {
                                    int number = int.Parse(value);
                                    field.SetValue(instance, number);
                                }
                                else if (fieldType == typeof(float))
                                {
                                    float number = float.Parse(value);
                                    field.SetValue(instance, number);
                                }
                                else if (fieldType == typeof(string))
                                    field.SetValue(instance, value);
                                else if (fieldType == typeof(bool))
                                    field.SetValue(instance, value == "TRUE");
                                else if(fieldType == typeof(char))
                                    field.SetValue(instance, value.ToChar());
                                else
                                    throw new Exception("Invalid Type To Deserialise");
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
            return instance;
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
                    parsed += $"{SPACE}\"{fieldValues[i].Name}\":{NULL};\n";
                    continue;
                }

                if (!fieldValues[i].FieldType.IsArray)
                {
                    switch(fieldValues[i].FieldType)
                    {
                        case var dataType when dataType == typeof(bool):
                            parsed += $"{SPACE}\"{fieldValues[i].Name}\":{((bool)fieldValues[i].GetValue(instance) == true).ToString().ToUpper()};\n";
                            break;
                        case var dataType when dataType == typeof(int) || dataType == typeof(float) || dataType == typeof(string) || dataType == typeof(char):
                            parsed += $"{SPACE}\"{fieldValues[i].Name}\":{fieldValues[i].GetValue(instance)};\n";
                            break;
                    }  
                }
                else
                {
                    dynamic arr = fieldValues[i].GetValue(instance);
                    string values = "[";

                    for (int k = 0; k < arr.Length; k++)
                    {
                        if (fieldValues[i].FieldType == typeof(string[]) || fieldValues[i].FieldType == typeof(char[]))
                        {
                            if (k + 1 != arr.Length)
                                values += $"'{arr[k]}',";
                            else
                                values += $"'{arr[k]}'";
                        }
                        else if (fieldValues[i].FieldType == typeof(bool[]))
                        {
                            if (k + 1 != arr.Length)
                                values += $"{arr[k].ToString().ToUpper()},";
                            else
                                values += $"{arr[k].ToString().ToUpper()}";
                        }
                        else if(fieldValues[i].FieldType == typeof(int[]) || fieldValues[i].FieldType == typeof(float[]))
                        {
                            if (k + 1 != arr.Length)
                                values += $"{arr[k]},";
                            else
                                values += $"{arr[k]}";
                        }
                        else
                            throw new Exception("Invalid Type To Serialise");
                    }
                    values += "]";

                    parsed += $"\n{SPACE}\"{fieldValues[i].Name}\":{values};\n";
                }
            }
            parsed += "}\n";

            Console.WriteLine("Done\n");

            return parsed;
        }
    }
}
