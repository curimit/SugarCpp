@echo off
java org.antlr.Tool SugarCpp.g
java org.antlr.Tool SugarWalker.g
echo %ERRORLEVEL%
if ERRORLEVEL 1 ( pause )