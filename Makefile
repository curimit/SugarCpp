# Makefile

LEX=flex
YACC=bison -d
CC=g++ -O2
FLAGS=-std=gnu++11 -DYYERROR_VERBOSE -DYYDEBUG
#SC=~/Work/SugarCpp-C\#/src/SugarCpp.CommandLine/bin/Debug/SugarCpp.CommandLine.exe
#SC=/tmp/SugarCpp
SC=SugarCpp

BUILD_DIR=bin
TARGET=./SugarCpp

scSources=$(shell find src -name "*.sc" | sed 's/^src\///g')
TMP_CPPS=$(scSources:.sc=.cpp) yacc/lex.yy.cpp yacc/yacc.tab.cpp

cppSources=$(addprefix src/, $(TMP_CPPS))
cppHeaders=$(filter-out src/yacc/lex.yy.h, $(cppSources:.cpp=.h))
# `lex.yy.cc` doesn't have a correspondent header file

OBJS=$(addprefix $(BUILD_DIR)/, $(TMP_CPPS:.cpp=.o))
DEPS=$(OBJS:.o=.d)

.PHONY: all run clean
.SECONDARY:

all: $(cppSources) $(TARGET)

$(TARGET): $(OBJS)
	@echo 'linking ...'
	@$(CC) $(FLAGS) $^ -o $@

run: SugarCpp
	@clear
	@echo "[result]"
	@./$(TARGET)

sinclude $(DEPS)

# object
$(BUILD_DIR)/%.o: src/%.cpp
	@mkdir -p $(dir $@)
	@echo "[cc] $< ..."
	@$(CC) $(FLAGS) -c $< -o $@

# dependency
$(BUILD_DIR)/%.d: src/%.cpp $(cppHeaders)
	@mkdir -p $(dir $@)
	@echo "[dep] $< ..."
	@$(CC) $(FLAGS) -MM -MT "$(@:.d=.o)" "$<" > "$@"

# SugarCpp
src/%.cpp src/%.h: src/%.sc
	@echo "[Sugar] $< ..."
	@$(SC) -nocode $<

# yacc / lex
src/yacc/yacc.tab.h src/yacc/yacc.tab.cpp: src/yacc/yacc.ypp
	$(YACC) $< --defines=yacc.tab.h		# use the right header file name
	@mv yacc.tab.* src/yacc

src/yacc/lex.yy.cpp: src/yacc/lex.l
	$(LEX) -o $@ $<

clean:
	@rm -rf bin/
	@rm -f $(cppSources) $(cppHeaders)