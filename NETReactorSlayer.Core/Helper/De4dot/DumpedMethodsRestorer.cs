﻿/*
    Copyright (C) 2011-2015 de4dot@gmail.com

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

using System.Collections.Generic;
using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.PE;

namespace NETReactorSlayer.Core.Helper.De4dot;

public class DumpedMethodsRestorer : IRowReader<RawMethodRow>, IColumnReader, IMethodDecrypter
{
    private readonly DumpedMethods dumpedMethods;
    private ModuleDefMD module;

    public DumpedMethodsRestorer(DumpedMethods dumpedMethods) => this.dumpedMethods = dumpedMethods;

    public ModuleDefMD Module
    {
        set => module = value;
    }

    public bool ReadColumn(MDTable table, uint rid, ColumnInfo column, out uint value)
    {
        if (table.Table == Table.Method)
            if (TryReadRow(rid, out var row))
            {
                value = row[column.Index];
                return true;
            }

        value = 0;
        return false;
    }

    public bool GetMethodBody(
        uint rid, RVA rva, IList<Parameter> parameters, GenericParamContext gpContext, out MethodBody methodBody)
    {
        var dm = GetDumpedMethod(rid);
        if (dm == null)
        {
            methodBody = null;
            return false;
        }

        methodBody = MethodBodyReader.CreateCilBody(module, dm.code, dm.extraSections, parameters, dm.mhFlags,
            dm.mhMaxStack, dm.mhCodeSize, dm.mhLocalVarSigTok, gpContext);
        return true;
    }

    public bool TryReadRow(uint rid, out RawMethodRow row)
    {
        var dm = GetDumpedMethod(rid);
        if (dm == null)
        {
            row = default;
            return false;
        }

        row = new RawMethodRow(dm.mdRVA, dm.mdImplFlags, dm.mdFlags, dm.mdName, dm.mdSignature, dm.mdParamList);
        return true;
    }

    private DumpedMethod GetDumpedMethod(uint rid) => dumpedMethods.Get(0x06000000 | rid);
}