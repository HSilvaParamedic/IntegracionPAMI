using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DevOps
{
    public bool Resultado { get; set; }
    public string DescripcionError { get; set; }

    public DevOps(bool pResultado, string pDescripcionError)
    {
        this.Resultado = pResultado;
        this.DescripcionError = pDescripcionError;
    }

    public DevOps(string pDescripcionError)
    {
        this.Resultado = false;
        this.DescripcionError = pDescripcionError;
    }

    public DevOps()
    {
        this.Resultado = true;
        this.DescripcionError = "";
    }

}