#!/bin/bash
# Author : Vasil Peevski
# Copyright (c) discoverita.com
# Script follows here:

source msgUtil

# Some Backup stuff
declare backupDir="${HOME}/Backup"
declare doBackup=false
declare doDeleteScript=false

while getopts "rbd:" opt; do
   case $opt in
      b)
		     doBackup=true
      ;;
   	  d)
		     backupDir="${OPTARG}"
		     doBackup=true
		  ;;
      r)
         doDeleteScript=true
      ;;
   esac
done
shift $(( OPTIND - 1 ))

# CS
declare -r scriptname="$1"
declare -r bindir="${HOME}/bin"
declare -r scriptfullpath="${bindir}/${scriptname}"
declare -r outputlog="${bindir}/output_log"
declare -r editor="/usr/bin/vi"
#editor="/Applications/Sublime Text.app/Contents/MacOS/Sublime Text"

exec 3>&1
exec 4>&2
exec &> >(tee -a "${outputlog}")

# -r available -> delete script file if exists
if [[ $doDeleteScript = true ]]; then
   if [[ -e ${scriptfullpath} && -f ${scriptfullpath} ]]; then 
      rm "${scriptfullpath}"
      if [[ ${?} =  0 ]]; then
         printSucc "File ${scriptfullpath} deleted successfully: Exit code: ${?}"
      fi
      exit ${?}
   else
      exitErr "Please provide an existing file name to delete!"
   fi
fi

printHeader "CREATE SCRIPT EXECUTION STARTED: ${0} ${*}"

# Check script file name is valid.
[[ ! $scriptname ]] && exitErr "Please enter valid script name"

# Check script file name is allready defined command in PATH.
if which $scriptname > /dev/null 2>&1; then
   exitErr "A command with name ${scriptname} is allready defined"
fi

# Check is ~/bin exists. Creates it if not.
if [[ ! -d $bindir ]]; then
   if mkdir "${bindir}"; then
      printSucc "Directory created: \"${bindir}\""
   else
      exitErr "Failed to create directory ${bindir}"
   fi   
fi

# Check is file allredy exists.
if [[ -e $scriptfullpath ]]; then
   exitErr "File \"${scriptfullpath}\" Allready exists"
fi

# Create file.
if echo "#!/bin/bash" > "$scriptfullpath"; then
   echo "source msgUtil" >> "${scriptfullpath}"
   echo "printHeader \"${scriptfullpath} execution start\"" >> "${scriptfullpath}"
   printSucc "File \"${scriptfullpath}\" was created"
else
   exitErr "Unable to create file \"${scriptfullpath}\""
fi

# Grant permissions.
if chmod u+x "${scriptfullpath}"; then
   printSucc "File \"${scriptfullpath}\" is granted executable permission for user ${USER}"
else
   printWarn "Unable to grant executable permission of file \"${scriptfullpath}\" for user ${USER}!"
fi

# Creating backup of bin direcory
if [[ $doBackup = true ]]; then
   backup -d "${backupDir}/bin" "${scriptfullpath}" "${outputlog}"
fi
printSucc "Exit code: ${?}"

# Turn off redirect by reverting STDOUT and STDERR. Closing FH3 and FH4
exec 1>&3-
exec 2>&4-

# Open new script file in editor.
"$editor" "$scriptfullpath"

exit 0
