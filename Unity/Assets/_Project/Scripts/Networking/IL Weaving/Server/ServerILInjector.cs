using System;
using System.Linq;
using Mewlist.Weaver;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class ServerILInjector : IILInjector
    {
        public void Validate(ICustomAttribute customAttribute)
        {
        }

        public void Inject(CustomAttribute customAttribute, ModuleDefinition moduleDefinition, MethodDefinition methodDefinition)
        {
            Type networkManagerType = typeof(NetworkManager);  
            MethodReference singletonRef = moduleDefinition.ImportReference(networkManagerType.GetMethod("get_Singleton"));
            MethodReference isListeningRef = moduleDefinition.ImportReference(networkManagerType.GetMethod("get_IsListening"));
            MethodReference isServerRef = moduleDefinition.ImportReference(networkManagerType.GetMethod("get_IsServer"));
            
            Type debugType = typeof(Debug);
            MethodReference logErrorRef = moduleDefinition.ImportReference(debugType.GetMethod("LogError", new []{ typeof(object) }));
            
            ILProcessor processor = methodDefinition.Body.GetILProcessor();
            Instruction first = methodDefinition.Body.Instructions.First();
            TypeReference returnTypeRef = methodDefinition.ReturnType;
 
            // Instruction returnInstruction = Instruction.Create(OpCodes.Ldnull); 
            Instruction returnInstruction1 = Instruction.Create(OpCodes.Ldstr, "No singleton"); 
            Instruction returnInstruction2 = Instruction.Create(OpCodes.Ldstr, "Not connected"); 
            
            // // If null, return
            processor.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, "On commence")); 
            processor.InsertBefore(first, Instruction.Create(OpCodes.Call, logErrorRef));
             
            
            
            processor.InsertBefore(first, Instruction.Create(OpCodes.Callvirt, singletonRef));  
            processor.InsertBefore(first, Instruction.Create(OpCodes.Brfalse_S, returnInstruction1));
            // // If false, return
            processor.InsertBefore(first, Instruction.Create(OpCodes.Callvirt, singletonRef)); 
            processor.InsertBefore(first, Instruction.Create(OpCodes.Callvirt, isListeningRef)); 
            processor.InsertBefore(first, Instruction.Create(OpCodes.Brfalse_S, returnInstruction2)); 
            // // If true, continue
            processor.InsertBefore(first, Instruction.Create(OpCodes.Callvirt, singletonRef));  
            processor.InsertBefore(first, Instruction.Create(OpCodes.Callvirt, isServerRef)); 
            processor.InsertBefore(first, Instruction.Create(OpCodes.Brtrue_S, first));
            // All conditions pass, return and log error because client called this method
            processor.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, "Only the server can invoke a Server method")); 
            processor.InsertBefore(first, Instruction.Create(OpCodes.Call, logErrorRef));
            
            
            processor.InsertBefore(first, returnInstruction1);
            processor.InsertBefore(first, Instruction.Create(OpCodes.Call, logErrorRef));
            processor.InsertBefore(first, Instruction.Create(OpCodes.Ret));
            
            processor.InsertBefore(first, returnInstruction2);
            processor.InsertBefore(first, Instruction.Create(OpCodes.Call, logErrorRef));
            processor.InsertBefore(first, Instruction.Create(OpCodes.Ret));
            // processor.InsertBefore(first, Instruction.Create(OpCodes.Ret));  
        }
    } 
}