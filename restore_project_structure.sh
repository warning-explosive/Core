#!/bin/bash
for file in $(find . -name '*.csproj' -exec basename {} \;)
do
    dest=$(egrep -o ", \"(.*${file})" Core.sln | cut -d \" -f 2)
    dest=${dest//\\//}
    mkdir -p `dirname ${dest}`
    mv "${file}" "${dest}"
done