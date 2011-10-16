/*
    Copyright (C) 2011 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace de4dot.blocks {
	public class Blocks {
		MethodDefinition method;
		IList<VariableDefinition> locals;
		MethodBlocks methodBlocks;

		public MethodBlocks MethodBlocks {
			get { return methodBlocks; }
		}

		public IList<VariableDefinition> Locals {
			get { return locals; }
		}

		public MethodDefinition Method {
			get { return method; }
		}

		public Blocks(MethodDefinition method) {
			var body = method.Body;
			this.method = method;
			this.locals = body.Variables;
			methodBlocks = new InstructionListParser(body.Instructions, body.ExceptionHandlers).parse();
		}

		public void deobfuscateLeaveObfuscation() {
			foreach (var scopeBlock in getAllScopeBlocks(methodBlocks))
				scopeBlock.deobfuscateLeaveObfuscation();
		}

		public int deobfuscate() {
			foreach (var scopeBlock in getAllScopeBlocks(methodBlocks))
				scopeBlock.deobfuscate(this);

			int numDeadBlocks = removeDeadBlocks();

			foreach (var scopeBlock in getAllScopeBlocks(methodBlocks)) {
				scopeBlock.mergeBlocks();
				scopeBlock.repartitionBlocks();
				scopeBlock.deobfuscateLeaveObfuscation();
			}

			return numDeadBlocks;
		}

		IEnumerable<ScopeBlock> getAllScopeBlocks(ScopeBlock scopeBlock) {
			var list = new List<ScopeBlock>();
			list.Add(scopeBlock);
			list.AddRange(scopeBlock.getAllScopeBlocks());
			return list;
		}

		public int removeDeadBlocks() {
			return new DeadBlocksRemover(methodBlocks).remove();
		}

		public void getCode(out IList<Instruction> allInstructions, out IList<ExceptionHandler> allExceptionHandlers) {
			new CodeGenerator(methodBlocks).getCode(out allInstructions, out allExceptionHandlers);
		}
	}
}