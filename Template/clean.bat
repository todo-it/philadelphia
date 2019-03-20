rem for local template development

del ForCSharp\*.user
rmdir ForCSharp\.vs /s /q
rmdir ForCSharp\packages /s /q
rmdir ForCSharp\PhiladelphiaPowered.Client\bin /s /q
rmdir ForCSharp\PhiladelphiaPowered.Client\obj /s /q
rmdir ForCSharp\PhiladelphiaPowered.Client\Bridge\output /s /q
del ForCSharp\PhiladelphiaPowered.Client\*.user

rmdir ForCSharp\PhiladelphiaPowered.Domain\bin /s /q
rmdir ForCSharp\PhiladelphiaPowered.Domain\obj /s /q
del ForCSharp\PhiladelphiaPowered.Domain\*.user

rmdir ForCSharp\PhiladelphiaPowered.Server\bin /s /q
rmdir ForCSharp\PhiladelphiaPowered.Server\obj /s /q
del ForCSharp\PhiladelphiaPowered.Server\*.user

rmdir ForCSharp\PhiladelphiaPowered.Services\bin /s /q
rmdir ForCSharp\PhiladelphiaPowered.Services\obj /s /q
del ForCSharp\PhiladelphiaPowered.Services\*.user
