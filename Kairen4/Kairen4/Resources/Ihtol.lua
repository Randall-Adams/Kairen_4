--[[ Meta-Data
Meta-Data-Version: 1.0

Code-Name: Ihtol
Code-Type: LUA Code
Code-Version: 1.0
Code-Description: My Cheat Engine auto-record loader. This formatted version was designed for loading a single lua script record. A FAPI File.
Code-Author: Robert Randazzio
]]--
function onOpenProcess()
    local timer = createTimer(nil, false)
    timer.Interval = 500
    timer.OnTimer = function(timer)
        local addresslist = getAddressList()
        local key = addresslist.getMemoryRecordByDescription("Main.lua - World Population")
        if key == nil then
        else
            key.Active = true
            timer.Destroy()
            return true
        end
    end
    timer.Enabled = true
end
