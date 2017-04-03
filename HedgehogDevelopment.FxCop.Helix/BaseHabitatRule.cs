using Microsoft.FxCop.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;

namespace HedgehogDevelopment.FxCop.Helix
{
    public abstract class BaseHelixRule : BaseIntrospectionRule
    {
#if DEBUG
        public static object _debugLock = new object();
#endif

        protected abstract string BaseNamespaceMoniker { get; }
        internal abstract void TestObjectValue(UniqueProblemCollection problems, SourceContext? sourceContext, string baseNamespace, object value, string messageFormat);

        public BaseHelixRule(string name) : base(name, "HedgehogDevelopment.FxCop.Helix.RuleMetadata", typeof(VerifyProjectStructure).Assembly)
        {
        }

        public override TargetVisibilities TargetVisibility
        {
            get
            {
                return TargetVisibilities.All;
            }
        }

        /// <summary>
        /// Returns an enumarable of instructions that are valid for testing
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        protected IEnumerable<Instruction> GetValidInstructions(InstructionCollection instructions)
        {
            int totalInstructions = instructions.Count;

            for (int instructionCount = 0; instructionCount != instructions.Count; instructionCount++)
            {
                if (instructionCount < totalInstructions - 1)
                {
                    //if the next instruction is a return, we don't want to evaluate the current instruction. This prevents the closing } of a function from showing up in the error list
                    if (instructions[instructionCount + 1].OpCode == OpCode.Ret)
                    {
                        continue;
                    }
                }

                yield return instructions[instructionCount];
            }
        }

        /// <summary>
        /// Return the base namespace that the objects belong to. This assumes that namespaces follow the format:
        /// 
        /// [SomeNamespacePrefix].[SomeNamespacePrefix].[n...].[BaseNamespaceMoniker].[SomeNamespacePostfix].[n...]
        /// 
        /// This will return the namespace that exists up to [SomeNamespacePostfix]
        /// 
        /// </summary>
        /// <param name="currentTypeNamespace"></param>
        /// <returns></returns>
        protected string GetNamespaceFromTypeName(string currentTypeNamespace, string namespaceMoniker)
        {
            int monikerEnd = -1;

            //If the namespace ends with the moniker or is equal to the moniker we are done
            if (currentTypeNamespace == namespaceMoniker || currentTypeNamespace.EndsWith("." + namespaceMoniker))
            {
                return currentTypeNamespace;
            }

            //Find the moniker if the namespace starts with the moniker (not recommended)
            if (currentTypeNamespace.StartsWith(namespaceMoniker + "."))
            {
                monikerEnd = namespaceMoniker.Length + 1;
            }

            //See if the moniker is in the middle of the namespace
            int monikerPos = currentTypeNamespace.IndexOf("." + namespaceMoniker + ".");
            if (monikerPos != -1)
            {
                monikerEnd = monikerPos + namespaceMoniker.Length + 1;
            }

            if (monikerEnd == -1)
            {
                //No moniker found. We can ignore
                return null;
            }

            //Find the end of the namespace after the moniker
            int baseEnd = currentTypeNamespace.IndexOf(".", monikerEnd + 1);
            if (baseEnd == -1)
            {
                //if there is no additional namespaces after namespace postfix, return the whole thing
                return currentTypeNamespace;
            }

            //Return up to the namespace Postfix
            return currentTypeNamespace.Substring(0, baseEnd);
        }

        /// <summary>
        /// Returns the namespace of an object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected void TestObjectNamespaces(object value, Action<string> namespaceActionTest)
        {
            if (value is Expression)
            {
                //Expression type is in Type.Fullname
                Expression expressionValue = value as Expression;

                TestObjectNamespaces(expressionValue.Type, namespaceActionTest);
            }
            else if (value is TypeNode)
            {
                //Typenodes use Fullname
                TypeNode typeNodeValue = value as TypeNode;

                if (typeNodeValue.IsGeneric)
                {
                    if (typeNodeValue.Template != null)
                    {
                        namespaceActionTest(typeNodeValue.Template.FullName);
                    }

                    if (typeNodeValue.StructuralElementTypes != null)
                    {
                        foreach (TypeNode structuralElementType in typeNodeValue.StructuralElementTypes)
                        {
                            TestObjectNamespaces(structuralElementType, namespaceActionTest);
                        }
                    }
                }
                else
                {
                    namespaceActionTest(typeNodeValue.FullName);
                }
            }
            else if (value is InstanceInitializer)
            {
                InstanceInitializer instanceInitializerValue = value as InstanceInitializer;

                TestObjectNamespaces(instanceInitializerValue.DeclaringType, namespaceActionTest);
            }
            else if (value is Method)
            {
                Method methodValue = value as Method;

                TestObjectNamespaces(methodValue.DeclaringType, namespaceActionTest);

                foreach (Parameter parameter in methodValue.Parameters)
                {
                    TestObjectNamespaces(parameter.Type, namespaceActionTest);
                }

                if (methodValue.IsGeneric)
                {
                    if (methodValue.TemplateArguments != null)
                    {
                        foreach (TypeNode genericType in methodValue.TemplateArguments)
                        {
                            TestObjectNamespaces(genericType, namespaceActionTest);
                        }
                    }

                    if (methodValue.TemplateParameters != null)
                    {
                        foreach (TypeNode genericType in methodValue.TemplateParameters)
                        {
                            TestObjectNamespaces(genericType, namespaceActionTest);
                        }
                    }
                }
            }
            else if (value is Node)
            {
                //Everything else returns the type from ToString()
                namespaceActionTest(value.ToString());
            }
            else
            {
                //non-FxCop objects return the value of their type
                namespaceActionTest(value.GetType().FullName);
            }
        }

        /// <summary>
        /// Parses member functions looking for bad namespaces
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public override ProblemCollection Check(Member member)
        {
#if DEBUG
            lock (_debugLock)
            {
#endif
                UniqueProblemCollection problems = new UniqueProblemCollection();

                //Check to see if instructions in a Method are illeagal
                if (member is Method)
                {
                    Method method = member as Method;

                    Check(problems, method);
                }
                else if (member is Field)
                {
                    Field field = member as Field;

                    Check(problems, field);
                }
                else if (member is PropertyNode)
                {
                    PropertyNode property = member as PropertyNode;

                    Check(problems, property);
                }
                else if (member is EventNode)
                {
                    EventNode eventNode = member as EventNode;

                    string currentTypeNamespace = eventNode.DeclaringType.FullName;
                    string baseNamespace = GetNamespaceFromTypeName(currentTypeNamespace, BaseNamespaceMoniker);

                    if (!string.IsNullOrEmpty(baseNamespace))
                    {
                        CallTestObjectValue(problems,
                            null,
                            baseNamespace,
                            eventNode.HandlerType,
                            string.Format("Event {0} in namespace {{0}} may not reference namespace {{1}}", eventNode.Name));
                    }
                }
                else
                {
                    string type = member.DeclaringType.FullName;
                }

                return problems.Problems;
#if DEBUG
            }
#endif
        }

        private void CallTestObjectValue(UniqueProblemCollection problems, SourceContext? sourceContext, string baseNamespace, object type, string messageFormat)
        {
            try
            {
                TestObjectValue(problems, sourceContext, baseNamespace, type, messageFormat);
            }
            catch (Exception ex)
            {
                Resolution res = new Resolution("Exception checking code {0}\n{1}", ex.Message, ex.StackTrace);

                if (sourceContext.HasValue)
                {
                    problems.Add(new Problem(res, sourceContext.Value));
                }
                else
                {
                    problems.Add(new Problem(res));
                }
            }
        }


        /// <summary>
        /// Checks properties for errors
        /// </summary>
        /// <param name="problems"></param>
        /// <param name="property"></param>
        private void Check(UniqueProblemCollection problems, PropertyNode property)
        {
            string currentTypeNamespace = property.DeclaringType.FullName;
            string baseNamespace = GetNamespaceFromTypeName(currentTypeNamespace, BaseNamespaceMoniker);

            if (!string.IsNullOrEmpty(baseNamespace))
            {
                CallTestObjectValue(problems,
                    null,
                    baseNamespace,
                    property.Type,
                    string.Format("Property {0} in namespace {{0}} may not reference namespace {{1}}", property.Name));
            }
        }

        /// <summary>
        /// Checks fields for errors
        /// </summary>
        /// <param name="problems"></param>
        /// <param name="field"></param>
        private void Check(UniqueProblemCollection problems, Field field)
        {
            string currentTypeNamespace = field.DeclaringType.FullName;
            string baseNamespace = GetNamespaceFromTypeName(currentTypeNamespace, BaseNamespaceMoniker);

            if (field.Type.NodeType != NodeType.DelegateNode)
            {
                if (!string.IsNullOrEmpty(baseNamespace))
                {
                    CallTestObjectValue(problems,
                        null,
                        baseNamespace,
                        field.Type,
                        string.Format("Field {0} in namespace {{0}} may not reference namespace {{1}}", field.Name));
                }
            }
        }

        /// <summary>
        /// Performs chaks on methods
        /// </summary>
        /// <param name="problems"></param>
        /// <param name="method"></param>
        private void Check(UniqueProblemCollection problems, Method method)
        {
            string currentTypeNamespace = method.DeclaringType.FullName;
            string baseNamespace = GetNamespaceFromTypeName(currentTypeNamespace, BaseNamespaceMoniker);

            if (!string.IsNullOrEmpty(baseNamespace))
            {
                //Validate the return type
                CallTestObjectValue(problems,
                    null,
                    baseNamespace,
                    method.ReturnType,
                    "Return value in namespace {0} may not reference namespace {1}");

                foreach (Instruction instruction in GetValidInstructions(method.Instructions))
                {
                    //Some values are collections of values. We need to check each
                    if (instruction.Value is ICollection)
                    {
                        foreach (object instructionValue in (instruction.Value as ICollection))
                        {
                            if (instructionValue != null)
                            {
                                CallTestObjectValue(problems,
                                    instruction.SourceContext,
                                    baseNamespace,
                                    instructionValue,
                                    "Method in namespace {0} may not reference namespace {1}");
                            }
                        }
                    }
                    else
                    {
                        if (instruction.Value != null)
                        {
                            CallTestObjectValue(problems,
                                instruction.SourceContext,
                                baseNamespace,
                                instruction.Value,
                                "Method in namespace {0} may not reference namespace {1}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks parameters for problems
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override ProblemCollection Check(Parameter parameter)
        {
#if DEBUG
            lock (_debugLock)
            {
#endif
                UniqueProblemCollection problems = new UniqueProblemCollection();

                string currentTypeNamespace = parameter.DeclaringMethod.DeclaringType.FullName;
                string baseNamespace = GetNamespaceFromTypeName(currentTypeNamespace, BaseNamespaceMoniker);

                if (parameter.DeclaringMethod.Body.SourceContext.IsValid)
                {
                    if (!string.IsNullOrEmpty(baseNamespace))
                    {
                        CallTestObjectValue(problems,
                            null,
                            baseNamespace,
                            parameter.Type,
                            string.Format("Parameter {0} declared in a method in namespace {{0}} may not reference namespace {{1}}", parameter.Name));
                    }
                }

                return problems.Problems;
#if DEBUG
            }
#endif
        }

        /// <summary>
        /// Checks types for problems
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override ProblemCollection Check(TypeNode type)
        {
#if DEBUG
            lock (_debugLock)
            {
#endif
                UniqueProblemCollection problems = new UniqueProblemCollection();

                string currentTypeNamespace = type.FullName;
                string baseNamespace = GetNamespaceFromTypeName(currentTypeNamespace, BaseNamespaceMoniker);

                if (!string.IsNullOrEmpty(baseNamespace))
                {
                    string typeName;

                    //Determine the name of the type
                    switch (type.NodeType)
                    {
                        case NodeType.DelegateNode:
                            typeName = "Delegate";
                            break;
                        case NodeType.Interface:
                            typeName = "Interface";
                            break;
                        default:
                            typeName = "Class";
                            break;
                    }

                    if (type.BaseType != null)
                    {
                        CallTestObjectValue(problems, null, baseNamespace, type.BaseType, typeName + " in namespace {0} may not be based on a class from namespace {1}");
                    }

                    //Check implemented interfaces
                    foreach (InterfaceNode interfaceType in type.Interfaces)
                    {
                        CallTestObjectValue(problems, null, baseNamespace, interfaceType.FullName, typeName + " in namespace {0} may not implement an interface from namespace {1}");
                    }

                    if (type is DelegateNode)
                    {
                        DelegateNode delegateNode = type as DelegateNode;

                        CallTestObjectValue(problems, null, baseNamespace, delegateNode.ReturnType, "Delegate in namespace {0} may not have a return type from namespace {1}");

                        foreach (Parameter parameter in delegateNode.Parameters)
                        {
                            TestObjectValue(problems, null, baseNamespace, parameter.Type, "Delegate parameter in namespace {0} may not reference namespace {1}");
                        }
                    }
                }
                return problems.Problems;
#if DEBUG
            }
#endif
        }
    }
}
