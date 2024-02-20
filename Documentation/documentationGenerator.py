import os
import re

# SIMPLE FUNCTION TO CHECK IF WORD HAS NEWLINE (KINDA DUMB)
def checkForEndline(word):
    if word[len(word) - 1:len(word)] == '\n':
        return True
    return False

# INSERT YOUR PROJECT NAME HERE FOR INSERTION INTO DOCUMENTATION
projectName = "**__Honor's Project Simulation__**"

templateString = "# " + projectName + " **__Documentation__**\n\n## **Source Code Documentation**\n\n"

# SOURCE CODE FOLDER GOES HERE, RECURSIVELY SEARCHS SUB DIRECTORIES TO FIND ALL CODE FILES
sourceFolder = "C:/Users/gordo/OneDrive/Documents/COMP4522_Project/AISimulator/Assets/Scripts/"

def removeMarkdownProblemCharacters(line):
    newLine = line.replace('*', '')
    newLine = newLine.replace('_', '')
    newLine = newLine.replace('/', '')
    return newLine

newFile = open(removeMarkdownProblemCharacters(projectName) + "_DOCS.md", "w")
newFileContents = templateString

# LANGUAGE SPECIFIC LIST OF COMMENT STRINGS
potentialSearchTerms = ['//', '/*']

# TERRIBLE PLEASE FIX LATER 
def checkForSearchTerms(line):
    if len(line.replace(' ', '')) < 2: return -1
    if ((line.replace(' ', '')[0] + line.replace(' ', '')[1]) == potentialSearchTerms[0]) and checkForPrivateComment(line.replace(' ', '')):
        return 0
    if ((line.replace(' ', '')[0] + line.replace(' ', '')[1]) == potentialSearchTerms[1]) and checkForPrivateComment(line.replace(' ', '')):
        return 1
    return -1

def checkForPrivateComment(line):
    if len(line) < 3: return False
    if (line[2] == "*"): return True
    return False

def pruneStartSearchTerm(line):
    return line[2:len(line)]

def scanForVariables(line, newFileContents):
    # Getting both vars and functions right now
    if (re.search("^.+ .+;", line) or re.search("^.+ .+ = ", line) or re.search("^.+[)]$", line)) and (not re.search("GENERATED_BODY()", line) and not re.search("UCLASS()", line)):
        line = removeMarkdownProblemCharacters(line)
        newFileContents += "\t" + line + "\n"
    return newFileContents

def scanForFunctions(line, newFileContents):
    return newFileContents

def checkForCodeFile(line):
    supportedExtensions = ["cs", "h", "java"]
    if (len(line.split(".")) != 2): return False
    for i in range(len(supportedExtensions)):
        if (line.split(".")[1] == supportedExtensions[i]): return True
    

# MAIN LOOP FOR PROCESSING SOURCE CODE
bScanning = False
bOneMoreLine = False
endSearchTerm = '*/'
currDir = ""
for root, dirs, files in os.walk(sourceFolder):
    for file in files:
        print(root)
        if len(root.split('/')) > 0 and currDir != root.split("/")[len(root.split('/')) - 1].split("\\")[0]: # Fix to plain english
            currDir = root.split("/")[len(root.split('/')) - 1] # Fix to plain english
            newFileContents += "## " + currDir + "\n\n"
            print(currDir)

        if checkForCodeFile(file): # Ensures only reading source code files and not binary/config

            # ADD FILE NAME TO DOCUMENTATION
            newFileContents += "### " + file.split('.')[0] + "\n"
            newFileContents += "-------\n\n"

            # OPEN RELEVENT FILE FOR EDITING
            currFile = open(os.path.join(root, file), 'r')
            lines = currFile.readlines()

            # READ LINE BY LINE
            for line in lines:
                # REMOVE INDENTATION
                line = line.replace('\t', '')
                count = 0
                for char in line:
                    if char != " ":
                        break
                    count+=1
                line = line[count:len(line)]

                # SCAN ANY POTENTIAL COMMENT ON THE LINE
                if not bScanning:
                    if bOneMoreLine:
                        bOneMoreLine = False
                        line = removeMarkdownProblemCharacters(line)
                        newFileContents += "    " + line + "\n"
                    else:
                        searchTermIndex = checkForSearchTerms(line)
                        if searchTermIndex >= 0:
                            line = pruneStartSearchTerm(line)
                            line = removeMarkdownProblemCharacters(line)
                            newFileContents += line + "\n"
                            if searchTermIndex == 1: 
                                bScanning = True
                else:
                    if line[len(line) - 3:len(line)-1] == endSearchTerm:
                        bScanning = False
                        bOneMoreLine = True
                        line = removeMarkdownProblemCharacters(line)
                        newFileContents += line[0:len(line) - 3] + "\n"
                    else:
                        line = removeMarkdownProblemCharacters(line)
                        newFileContents += line + "\n"

                if not bScanning:
                    # SCAN AND ADD ALL VARIABLE DEFS TO DOCUMENTATION
                    # ToDo

                    # SCAN AND ADD ALL FUNCTION NAMES TO DOCUMENTATION
                    newFileContents = scanForFunctions(line, newFileContents)
            currFile.close()

newFile.write(newFileContents)
newFile.close()
