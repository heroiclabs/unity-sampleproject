--[[
 Copyright 2019 The Nakama Authors

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
--]]

local lt = {}

function lt.list_tab(tab, indent)
    if (tab == nil) then
        print("tab is nil")
        return
    end

    if type(tab) == "table" then
        for k, v in pairs(tab) do
            print((indent or "") .. tostring(k))
            lt.list_tab(v, (indent or "") .. "\t")
        end
    else
        print((indent or "") .. "\t" .. tostring(tab))
    end
end

return lt
