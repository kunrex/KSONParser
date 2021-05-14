using System;
using System.Reflection;
using System.Collections.Generic;
using HelpfulExtensions;
using System.Collections;

namespace KSON
{
    public class KsonParser
    {
        const string NULL = "NULL";
        const string SPACE = "   ";

        /*
        should convert ->
        {
            "x":0;
            "y":hello;
        }

        to ->

        class myClass
        {
            int x = 0;
            string y = "hello";
        }
        */

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

                        if (!field.FieldType.IsValidType(true))
                            throw new Exception("Invalide Type To Deserialise");

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

                                        case char _charecter when _charecter.IsValidToBeDeserialised_Array():
                                            currentValue += value[k];
                                            break;
                                        case char _charecter when _charecter.DeterminesEndOfSerialisedArray():
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
                            }
                        }

                        value = string.Empty;
                        break;
                    case char _char when _char.IsViableToBeSerialised():
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

        /*
        should convert ->

        class myClass
        {
            int x = 0;
            string y = "hello";
        }

        to ->

        {
            "x":0;
            "y":hello;
        }
        */
        public static string ToKson(object instance)
        {
            Console.WriteLine("Converting to Kson text\n");

            FieldInfo[] fieldValues = instance.GetType().GetFields();

            string parsed = "{\n";
            for(int i = 0;i < fieldValues.Length;i++)
            {
                if(!fieldValues[i].FieldType.IsValidType(true))
                    throw new Exception("Invalide Type To Serialise");

                if (fieldValues[i].GetValue(instance) == null)
                {
                    parsed += $"{SPACE}\"{fieldValues[i].Name}\":{NULL};\n";
                    continue;
                }

                if (fieldValues[i].FieldType.IsArray)
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
                        else if (fieldValues[i].FieldType == typeof(int[]) || fieldValues[i].FieldType == typeof(float[]))
                        {
                            if (k + 1 != arr.Length)
                                values += $"{arr[k]},";
                            else
                                values += $"{arr[k]}";
                        }
                    }
                    values += "]";

                    parsed += $"\n{SPACE}\"{fieldValues[i].Name}\":{values};\n";
                }
                else
                {
                    switch (fieldValues[i].FieldType)
                    {
                        case var dataType when dataType == typeof(bool):
                            parsed += $"{SPACE}\"{fieldValues[i].Name}\":{((bool)fieldValues[i].GetValue(instance) == true).ToString().ToUpper()};\n";
                            break;
                        case var dataType when dataType.IsValidType():
                            parsed += $"{SPACE}\"{fieldValues[i].Name}\":{fieldValues[i].GetValue(instance)};\n";
                            break;
                    }
                }
            }
            parsed += "}\n";

            Console.WriteLine("Done\n");

            return parsed;
        }
    }
}
