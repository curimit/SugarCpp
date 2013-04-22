@echo off
java org.antlr.Tool *.g
if ERRORLEVEL 1 ( pause )