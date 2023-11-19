echo "Doing a clean build..."
dotnet cake --target Build --clean
export GAME_DIRECTORY="$HOME/.steam/debian-installation/steamapps/common/SpiderHeck"
echo "Game directory: $GAME_DIRECTORY"
export MODWEAVER_COPY_TARGET="$GAME_DIRECTORY/modweaver/libs"
echo "Libs directory: $MODWEAVER_COPY_TARGET"
echo "Cleaning libs directory"
rm -rf $MODWEAVER_COPY_TARGET/*
echo "Copying files"
cp bin/0Harmony.dll $MODWEAVER_COPY_TARGET
cp bin/modweaver.core.dll $MODWEAVER_COPY_TARGET
cp bin/modweaver.preload.dll $MODWEAVER_COPY_TARGET
cp bin/Mono.Cecil.dll $MODWEAVER_COPY_TARGET
cp bin/Mono.Cecil.Mdb.dll $MODWEAVER_COPY_TARGET
cp bin/Mono.Cecil.Pdb.dll $MODWEAVER_COPY_TARGET
cp bin/Mono.Cecil.Rocks.dll $MODWEAVER_COPY_TARGET
cp bin/MonoMod.RuntimeDetour.dll $MODWEAVER_COPY_TARGET
cp bin/MonoMod.Utils.dll $MODWEAVER_COPY_TARGET
echo "Done!"