using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

partial class MyCardModel
{
    public MyCard FindById(int id)
    {
        return list.Find((c) => c.id == id);
    }
}
