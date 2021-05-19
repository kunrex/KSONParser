using System;
using System.Reflection;
using System.Collections.Generic;
using HelpfulExtensions;
using System.Collections;

namespace KSON
{
    public static class KsonParser
    {
        public const string NULL = "NULL";
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
            Type type = typeof(T);

            var instance = Activator.CreateInstance(type);//create instance of required type

            bool varNameFound = false, classOrStructVar = false;
            string varName = string.Empty, value = string.Empty;

            for (int i = 0; i < ksonString.Length; i++)//loop through all the characters in the entered string
            {
                switch (ksonString[i])
                {
                    case '{':
                        if (i != 0)//checks wether its the start of the parse
                        {
                            classOrStructVar = true;
                            value += '{';
                        }
                        break;
                    case '}':
                        if (i != ksonString.Length - 1)// checks wether its the end of the parse
                        {
                            classOrStructVar = false;
                            value += '}';
                        }
                        break;
                    case char c when c == '"' && !classOrStructVar://checks wether or not a variable name is being detected, if not set varName to ""
                        varNameFound = !varNameFound;
                        if (varNameFound)
                            varName = string.Empty;
                        break;
                    case char c when c == ';' && !classOrStructVar://a complete field with variable name + value has been found, parse the value and change it in the created instance and reset "varName" and "value". 
                        //all of this only happens if its not currently reading a class or struct variable
                        FieldInfo field = instance.GetType().GetField(varName);

                        if (!field.FieldType.IsValidType(true))//not a deserialisable type
                            if (field.FieldType.GetCustomAttribute<Serialisable>() == null)
                                throw new InvalidTypeException(false);

                        if (value == NULL)//the value is null
                        {
                            field.SetValue(instance, null);
                        }
                        else
                        {
                            Type fieldType = field.FieldType;
                            if (fieldType.GetCustomAttribute<Serialisable>() != null)//a serialsible struct or class
                            {
                                MethodInfo method = typeof(KsonParser).GetMethod(nameof(KsonParser.FromKson));
                                MethodInfo generic = method.MakeGenericMethod(fieldType);
                                var serialisedClass = generic.Invoke(null, new[] { value });//"creates" a new FromKson method 

                                field.SetValue(instance, serialisedClass);//sets the value of the instance
                            }
                            else
                            {
                                if (field.FieldType.IsArray)//type is array
                                {
                                    Type listType = typeof(List<>).MakeGenericType(fieldType.GetElementType());
                                    dynamic values = Activator.CreateInstance(listType);//create a list of elemant type of the variable being parsed

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
                                                values.Add(type.GetCustomAttribute<Serialisable>().Deserialise(currentValue, fieldType.GetElementType()));//adds the deserialised value to the list
                                                currentValue = string.Empty;//reset the string values
                                                break;
                                        }

                                        field.SetValue(instance, values.ToArray());//assign the elemants to the variable in the created instance of type T
                                    }
                                }
                                else
                                    field.SetValue(instance, type.GetCustomAttribute<Serialisable>().Deserialise(value, fieldType));//sets the value in the instance to the deserialised value
                            }
                        }

                        value = string.Empty;
                        break;
                    case char _char when _char.IsViableToBeSerialised() && !classOrStructVar://character can be deserialised
                        if (varNameFound)//if a variable name is being read
                            varName += _char;
                        else//variable values are being read
                            value += _char;
                        break;
                    default:
                        if (classOrStructVar)//if a class/struct variable is being parsed then add everything to the value
                            value += ksonString[i];
                        break;
                }
            }

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
        public static string ToKson(object instance, int indent = 1)
        {
            Type type = instance.GetType();

            if (type.GetCustomAttribute<Serialisable>() == null)
                throw new TypeNotSerialisable(type.ToString());

            FieldInfo[] fieldValues = instance.GetType().GetFields();//all the fields in the class
          
            string indentation = Indent(indent),prevIndentation = Indent(indent - 1), parsed = prevIndentation + "{\n";

            for (int i = 0; i < fieldValues.Length; i++)//loop through all the fields
            {
                if (!fieldValues[i].FieldType.IsValidType(true))//checks if field can be serialised or not
                    if (fieldValues[i].FieldType.GetCustomAttribute<Serialisable>() == null)
                        throw new InvalidTypeException(true);

                if (fieldValues[i].GetValue(instance) == null)//value of null 
                {
                    parsed += $"{indentation}\"{fieldValues[i].Name}\":{NULL};\n";
                    continue;
                }

                if (fieldValues[i].FieldType.GetCustomAttribute<Serialisable>() != null)
                {
                    parsed += "\n" + SPACE + $"\"{fieldValues[i].Name}\":\n";
                    parsed += ToKson(fieldValues[i].GetValue(instance), indent + 1);//recursive i know
                }
                else
                {
                    if (fieldValues[i].FieldType.IsArray)//is array
                    {
                        dynamic arr = fieldValues[i].GetValue(instance);//create a dynmaic array with the elemants
                        string values = "[";

                        for (int k = 0; k < arr.Length; k++)//loop through all the elamants and depending on the type, add them to the "value" string
                        {
                            if (k + 1 != arr.Length)//checks if a ',' should be added
                                values += $"{type.GetCustomAttribute<Serialisable>().Serialise(arr[k])},";
                            else
                                values += $"{type.GetCustomAttribute<Serialisable>().Serialise(arr[k])}";//adds the serialised value to the "value" string
                        }
                        values += "]";

                        parsed += $"\n{indentation}\"{fieldValues[i].Name}\":{values};\n";//add the value and variable to the parsed string
                    }
                    else
                    {
                        dynamic value = fieldValues[i].GetValue(instance);//create a dynamic value
                        parsed += $"{indentation}\"{fieldValues[i].Name}\":{type.GetCustomAttribute<Serialisable>().Serialise(value)};\n";//add the value and variable to the parsed string
                    }
                }
            }
            parsed += prevIndentation + "}" + (indent == 1 ? "" : ";") + "\n";//checks if its a nested class variable or not and adds a ';' depending on that

            return parsed;
        }

        private static string Indent(int value)//calculats indentation
        {
            string indentation = string.Empty;
            for (int i = 0; i < value; i++)
            {
                indentation += SPACE;
            }

            return indentation;
        }
    }
}
