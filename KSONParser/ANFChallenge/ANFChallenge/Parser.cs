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

            var instance = Activator.CreateInstance(type);//create instance of required type

            bool varNameFound = false;
            string varName = string.Empty, value = string.Empty;

            for(int i =0;i<ksonString.Length;i++)//loop through all the characters in the entered string
            {
                switch (ksonString[i])
                {
                    case char k when k == '{' || k == '}'://ignore
                        break;
                    case '"'://checks wether or not a variable name is being detected, if not set varName to ""
                        varNameFound = !varNameFound;
                        if (varNameFound)
                            varName = string.Empty;
                        break;
                    case ';'://a complete field with variable name + value has been found, parse the value and change it in the created instance and reset "varName" and "value"
                        FieldInfo field = instance.GetType().GetField(varName);

                        if (!field.FieldType.IsValidType(true))//not a deserialisable type
                            throw new Exception("Invalide Type To Deserialise");

                        if (value == NULL)//the value is null
                        {
                            field.SetValue(instance, null);
                        }
                        else
                        {
                            Type fieldType = field.FieldType;

                            if (field.FieldType.IsArray)//type is array
                            {
                                Type listType = typeof(List<>).MakeGenericType(fieldType.GetElementType());
                                dynamic list = Activator.CreateInstance(listType);//create a list of elemant type of the variable being parsed

                                string currentValue = string.Empty;

                                for (int k = 0; k < value.Length; k++)//loop through the "value" string to find the elemants of the array
                                {
                                    switch (value[k])
                                    {
                                        case '['://ignore
                                        case ' ':
                                            break;

                                        case char _charecter when _charecter.IsValidToBeDeserialised_Array()://elemant
                                            currentValue += value[k];
                                            break;
                                        case char _charecter when _charecter.DeterminesEndOfSerialisedArray()://end of the elemant, depending on the data type of the field, add respective values
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

                                            currentValue = string.Empty;//reset the string values
                                            break;
                                    }

                                    field.SetValue(instance, list.ToArray());//assign the elemants to the variable in the created instance of type T
                                }
                            }
                            else
                            {
                                if (fieldType == typeof(int))//depending on the variable type, assign the respective values
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
                    case char _char when _char.IsViableToBeSerialised()://character can be deserialised 
                        if (varNameFound)//if a variable name is being read
                            varName += _char;
                        else//variable values are being read
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

            FieldInfo[] fieldValues = instance.GetType().GetFields();//all the fields in the class

            string parsed = "{\n";
            for(int i = 0;i < fieldValues.Length;i++)//loop through all the fields
            {
                if(!fieldValues[i].FieldType.IsValidType(true))//checks if field can be serialised or not
                    throw new Exception("Invalide Type To Serialise");

                if (fieldValues[i].GetValue(instance) == null)//value of null 
                {
                    parsed += $"{SPACE}\"{fieldValues[i].Name}\":{NULL};\n";
                    continue;
                }

                if (fieldValues[i].FieldType.IsArray)//is array
                {
                    dynamic arr = fieldValues[i].GetValue(instance);//create a dynmaic array with the elemants
                    string values = "[";

                    for (int k = 0; k < arr.Length; k++)//loop through all the elamants and depending on the type, add them to the "value" string
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

                    parsed += $"\n{SPACE}\"{fieldValues[i].Name}\":{values};\n";//add the value and variable to the parsed string
                }
                else
                    parsed += $"{SPACE}\"{fieldValues[i].Name}\":{(fieldValues[i].FieldType == typeof(bool) ? fieldValues[i].GetValue(instance).ToString().ToUpper() : fieldValues[i].GetValue(instance))};\n";//add the value and variable to the parsed string
                }
            }
            parsed += "}\n";

            Console.WriteLine("Done\n");

            return parsed;
        }
    }
}
