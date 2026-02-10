using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoGuard.Core.Interfaces;

public interface ISignatureValidator
{
    bool IsValid(string payload, string? signatureWithPrefix, string secret);
}
