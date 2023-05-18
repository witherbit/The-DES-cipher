using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_DES_cipher.DES
{
    public class Block
    {
        public byte[] Source { get; set; }
        public byte[] IP { get; set; }
        public byte[] L0 { get; set; }
        public byte[] R0 { get; set; }
        public byte[] LEnd { get; set; }
        public byte[] REnd { get; set; }
        public List<byte[]> L { get; set; }
        public List<byte[]> R { get; set; }
        public List<byte[]> EP { get; set; }
        public List<byte[]> RoundKey { get; set; }
        public List<byte[]> XorToRoundKey { get; set; }
        public List<byte[]> SBox { get; set; }
        public List<byte[]> SBoxPermutation { get; set; }
        public List<byte[]> XorToL { get; set; }
        public byte[] LR { get; set; }
        public byte[] IPInverse { get; set; }
        public byte[] XorCBC { get; set; }
        public Block()
        {
            EP = new List<byte[]>();
            RoundKey = new List<byte[]>();
            XorToRoundKey = new List<byte[]>();
            SBox = new List<byte[]>();
            SBoxPermutation = new List<byte[]>();
            XorToL = new List<byte[]>();
            L = new List<byte[]>();
            R = new List<byte[]>();
        }
    }
}
