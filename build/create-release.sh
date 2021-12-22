# chmod +x create-release.sh

while true ; do
    case "$1" in
        -version )
            version=$2
            shift 2
        ;;
        *)
            break
        ;;
    esac 
done;

array=( ${version//./ } ) 

major="${array[0]}"
minor="${array[1]}"
patch="${array[2]}"

if [ -z "$major" ]; then
    echo "Invalid major version"
    exit 0
fi

if [ -z "$minor" ]; then
    echo "Invalid minor version"
    exit 0
fi

if [ -z "$patch" ]; then
   patch="0"
fi

tagName="$major.$minor.$patch"

branch=$(git branch --show-current)
echo "Current branch $branch"

if [ "main" != $branch ]; then
   echo "You can only create release from main"
   exit 0
fi 

haschanges=$(git status --porcelain|wc -l)
if [ 0 -eq $haschanges ]; then
   git fetch --all
else
   echo "Uncommited changes detected!"
   exit 0
fi 

git pull
git tag -a $tagName -m "Created tag $tagName"
echo "Created tag $tagName"
git push --set-upstream origin $tagName