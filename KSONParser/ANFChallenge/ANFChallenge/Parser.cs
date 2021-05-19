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
            Console.WriteLine("Converting to class instance\n");

            Type type = typeof(T);

            var instance = Activator.CreateInstance(type);//create instance of required type

            bool varNameFound = false;
            string varName = string.Empty, value = string.Empty;

            for (int i = 0; i < ksonString.Length; i++)//loop through all the characters in the entered string
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
                            throw new InvalidTypeException(false);

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

            Type type = instance.GetType();

            if (type.GetCustomAttribute<Serialisable>() == null)
                throw new FieldNotSerialisable(instance.GetType().ToString());

            FieldInfo[] fieldValues = instance.GetType().GetFields();//all the fields in the class

            string parsed = "{\n";
            for (int i = 0; i < fieldValues.Length; i++)//loop through all the fields
            {
                if (!fieldValues[i].FieldType.IsValidType(true))//checks if field can be serialised or not
                    throw new InvalidTypeException(true);

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
                        if (k + 1 != arr.Length)//checks if a ',' should be added
                            values += $"{type.GetCustomAttribute<Serialisable>().Serialise(arr[k])},";
                        else
                            values += $"{type.GetCustomAttribute<Serialisable>().Serialise(arr[k])}";//adds the serialised value to the "value" string
                    }
                    values += "]";

                    parsed += $"\n{SPACE}\"{fieldValues[i].Name}\":{values};\n";//add the value and variable to the parsed string
                }
                else
                {
                    dynamic value = fieldValues[i].GetValue(instance);//create a dynamic value
                    parsed += $"{SPACE}\"{fieldValues[i].Name}\":{type.GetCustomAttribute<Serialisable>().Serialise(value)};\n";//add the value and variable to the parsed string
                }
            }
            parsed += "}\n";

            Console.WriteLine("Done\n");

            return parsed;
        }
    }
}
