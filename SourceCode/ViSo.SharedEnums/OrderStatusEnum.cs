using System.ComponentModel;

namespace ViSo.SharedEnums
{
    public enum OrderStatusEnum
    {
        [Description("Open")]
        Open = 1,

        [Description("On Backorder")]
        OnBackOrder = 2,

        [Description("Ready for Invoicing")]
        ReadyForInvoicing = 3,

        [Description("Invoiced")] 
        Invoiced = 4,

        [Description("Closed")] 
        Closed = 5,

        [Description("Canceled")] 
        Canceled = 7,

        [Description("Released")] 
        Released = 8,

        [Description("Backorder Released")] 
        BackOrderReleased = 9,

        [Description("Sub-Agent Invoiced")]
        SubagentInvoiced = 10
    }
}
