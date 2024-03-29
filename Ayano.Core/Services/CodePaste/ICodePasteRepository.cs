﻿using System.Collections.Generic;

namespace Ayano.Core.Services.CodePaste
{
    public interface ICodePasteRepository
    {
        UserCodePaste GetPaste(int id);
        IEnumerable<UserCodePaste> GetPastes();
        void AddPaste(UserCodePaste paste);
    }
}
