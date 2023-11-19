GAME_DIRECTORY=""
MODWEAVER_COPY_TARGET=""
SHOULD_CONTINUE=""
if [[ -z "$1" ]];
then
	echo "No Arguments Provided!"
	echo "wininstall <GAME_DIRECTORY>"
fi
if [[ "$1" ]];
then

	# proceed with installation
	GAME_DIRECTORY=$1
	MODWEAVER_CORE_TARGET="$GAME_DIRECTORY/modweaver"
	MODWEAVER_LIBS_TARGET="$MODWEAVER_CORE_TARGET/libs"
	echo "Cleaning libs directory!" # Clean up previous build
	echo -e -n "\033[1\033[4m$MODWEAVER_CORE_TARGET\033[0m | IS THIS THE CORRECT DIRECTORY?\033[0m <y/n>: "
	read con
	if [[ "$con" == "Y" || "$con" == "y" ]];
	then
		if [ -f "$MODWEAVER_CORE_TARGET" ]; then
			echo "REMOVING FILES IN $MODWEAVER_CORE_TARGET"
			rm -r "$MODWEAVER_CORE_TARGET/*" # seems dangerous?
			SHOULD_CONTINUE="YES"
		else
			echo "Folder doesn't exist, creating"
			# gotta make it
			mkdir "$MODWEAVER_CORE_TARGET"
			mkdir "$MODWEAVER_LIBS_TARGET"
			SHOULD_CONTINUE="YES"
		fi
	else
		# so they don't remove system 32 :9
		echo "Please input the correct directories <GAME_DIRECTORY>"
	fi
	if [[ "$SHOULD_CONTINUE" == "YES" ]];
	then
		# we now build the solution
		# don't waste time building if they put in wrong dir
		dotnet cake --nologo -v q --property WarningLevel=0 --target Build --clean ## -v q(verbose quest)
		echo "Copying files"
		cp "./bin/0Harmony.dll" "$MODWEAVER_LIBS_TARGET"
		cp "./bin/modweaver.core.dll" "$MODWEAVER_LIBS_TARGET"
		cp "./bin/modweaver.preload.dll" "$MODWEAVER_LIBS_TARGET"
		cp "./bin/Mono.Cecil.dll" "$MODWEAVER_LIBS_TARGET"
		cp "./bin/Mono.Cecil.Mdb.dll" "$MODWEAVER_LIBS_TARGET"
		cp "./bin/Mono.Cecil.Pdb.dll" "$MODWEAVER_LIBS_TARGET"
		cp "./bin/Mono.Cecil.Rocks.dll" "$MODWEAVER_LIBS_TARGET"
		cp "./bin/MonoMod.RuntimeDetour.dll" "$MODWEAVER_LIBS_TARGET"
		cp "./bin/MonoMod.Utils.dll" "$MODWEAVER_LIBS_TARGET"
		echo "Finished"
	fi
fi