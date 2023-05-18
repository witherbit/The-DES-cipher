using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_DES_cipher.DES
{
    public sealed class RoundKey
    {
        public int Round { get; set; }
        public byte[] Key { get; set; }
        public byte[] PC1 { get; set; }
        public byte[] C0 { get; set; }
        public byte[] D0 { get; set; }
        public byte[] C { get; set; }
        public byte[] D { get; set; }
        public byte[] PrePC2 { get; set; }
    }
}
