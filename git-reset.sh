# chmod +x git-reset.sh

# delete all releases
gh release list | sed 's/|/ /' | awk '{print $1, $8}' | while read -r line; do gh release delete -y "$line"; done

# delete all tags
git tag -d $(git tag -l)
git fetch
git push origin --delete $(git tag -l)
git tag -d $(git tag -l)

# clean git history
# remove the history from 
rm -rf .git

# recreate the repos from the current content only
git flow init -d
git add .
git commit -m "skip(): Initial commit"

# push to the github remote repos ensuring you overwrite history
git remote add origin git@github.com:sweepator/next.git
git push --set-upstream -f origin develop

git checkout master
git remote add origin git@github.com:sweepator/next.git
git merge develop
git push --set-upstream -f origin master
git checkout develop