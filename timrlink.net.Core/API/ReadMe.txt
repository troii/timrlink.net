-- Generation of Code from WSDL

fetch wsdl
 * http://timrsync.timr.com/timr/timrsync.wsdl
 or
 * svcutil /t:metadata http://timrsync.timr.com/timr/timrsync.wsdl

since .Net handles some attributes different than java we have to modify the wsdl a bit:
 * replace all occurances of 'minOccurs="0" maxOccurs="1"' with 'minOccurs="1" maxOccurs="1" nillable="true"'

generate Code in current folder with:
 * svcutil timrsync.wsdl /noConfig /internal /ct:System.Collections.Generic.List`1 /noStdLib /wrapped /tcv:Version35

add namespace at root:
 * namespace timrlink.net.Core.API {}

remove unavailable attributes:
 * regex: '^\s*(\[System.SerializableAttribute\(\)\]|\[System.ComponentModel.DesignerCategoryAttribute\("code"\)\])\r\n'

 replaces all arrays with lists:
 * regex
   * '(\w+)\[\]'
   * 'System.Collections.Generic.List<$1>'