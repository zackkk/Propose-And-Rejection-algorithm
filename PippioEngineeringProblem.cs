using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PippioEngineeringProblem {
    public class Matching{
        string[] people;
        string[] advertisers;
        Dictionary<string, List<string>> advertisersPreference; // preference list in descresing order for all people of each advertiser
        Dictionary<string, int> advertisersPreferenceIndex; // index of the preference list

        public Matching(string[] people, string[] advertisers) {
            advertisersPreferenceIndex = new Dictionary<string, int>();
            advertisersPreference = new Dictionary<string, List<string>>();
            this.people = people;
            this.advertisers = advertisers;
            GenerateAdvertisersPreferenceList();
        }

        // Propose-And-Rejection algorithm http://www.cs.princeton.edu/courses/archive/spr05/cos423/lectures/01stable-matching.pdf
        // advertisers(men) propose to people(women)
        public void PrintMaxMatching() {
            List<string> freeAdvertisers = new List<string>(advertisers);
            Dictionary<string, string> matched = new Dictionary<string, string>(); // match person with advertiser

            while (freeAdvertisers.Count > 0) {
                for (int i = 0; i < freeAdvertisers.Count; ++i) { // enumerator doesn't work since list would change
                    string freeAdvertiser = freeAdvertisers[i];
                    if (advertisersPreferenceIndex[freeAdvertiser] == people.Length) continue;

                    int preferredPersonIndex = advertisersPreferenceIndex[freeAdvertiser];
                    string preferredPerson = advertisersPreference[freeAdvertiser][preferredPersonIndex];

                    if (!matched.ContainsKey(preferredPerson))  // the person hasn't been matched
                    {
                        matched[preferredPerson] = freeAdvertiser;
                        freeAdvertisers.Remove(freeAdvertiser);
                    }
                    else  // the person has been matched
                    {
                        string matchedAdvertiser = matched[preferredPerson];

                        // the preferred person prefers the current advertiser than its previous matched advertiser
                        // this could also use the indexes of two advertisers of the preferredPerson, but it would exchange even they are equal
                        if (GetCTR(freeAdvertiser, preferredPerson) > GetCTR(matchedAdvertiser, preferredPerson))
                        {
                            matched[preferredPerson] = freeAdvertiser;
                            freeAdvertisers.Remove(freeAdvertiser);
                            freeAdvertisers.Add(matchedAdvertiser);
                        }
                    }
                    advertisersPreferenceIndex[freeAdvertiser]++; // consider the next person
                }
            }

            double sum = 0;
            foreach (KeyValuePair<string, string> entry in matched) {
                System.Console.WriteLine("<advertiser,person>:" + "<" + entry.Key + "," + entry.Value + ">");
                sum += GetCTR(entry.Key, entry.Value);
            }
            System.Console.WriteLine("Max CTR: " + sum);
        }

        // Generate preference list in descresing order for each advertiser
        void GenerateAdvertisersPreferenceList() {
            foreach (string advertiser in advertisers) {
                advertisersPreferenceIndex[advertiser] = 0;
                advertisersPreference[advertiser] = new List<string>(people);
            }
            foreach (string advertiser in advertisers) {
                advertisersPreference[advertiser].Sort((x, y) => -GetCTR(advertiser, x).CompareTo(GetCTR(advertiser, y)));
            }
        }

        // Return Click Through Rate (CTR) for a person and a advertiser
        public double GetCTR(string person, string advertiser) {
            int vowelCount = 0, consonantCount = 0;
            foreach (char c in person) {
                if (IsVowel(c)) {
                    vowelCount++;
                }
                else if ((c >= 'a' && c <= 'z') ||  (c >= 'A' && c <= 'Z')){ // skip spaces
                    consonantCount++;
                }
            }

            // If the length of the advertiser’s name is even, the base CTR is the number of vowels in  the person’s name multiplied by 1.5. 
            // If the length of the advertiser’s name is odd, the base CTR is the number of consonants  in the person’s name multiplied by 1.
            double CTR = (advertiser.Length % 2 == 0) ? 1.5 * vowelCount : consonantCount;

            // If the length of the advertiser’s name shares any common factors (besides 1) with the length of the person’s name, 
            // the CTR is increased by 50% above the base CTR.
            IList<int> person_len_factors = GetFactors(person.Length);
            IList<int> advertiser_len_factors = GetFactors(advertiser.Length);
            HashSet<int> person_len_factors_hashset = new HashSet<int>(person_len_factors);
            foreach (int factor in advertiser_len_factors) {
                if (factor != 1 && person_len_factors_hashset.Contains(factor)) {
                    CTR *= 1.5;
                    break;
                }
            }

            return CTR;
        }

        IList<int> GetFactors(int number){
            List<int> factors = new List<int>();
            int max = (int)Math.Sqrt(number);  //round down
            for (int factor = 1; factor <= max; ++factor) { // test 1 from square root
                if (number % factor == 0) {
                    factors.Add(factor);
                    if (factor != number / factor) { // Don't add the square root twice
                        factors.Add(number / factor);
                    }
                }
            }
            return factors;
        }

        bool IsVowel(char c) {
            return (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u') || 
                   (c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U');
        }

    }

    public class Test {
        public static void Main(string[] args) {
            string[] people, advertisers;
            if (args.Any()) {
                string advertisersPath = args[0], peoplePath = args[1];
                if (File.Exists(advertisersPath) && File.Exists(peoplePath)){
                     advertisers = File.ReadAllLines(advertisersPath);
                     people = File.ReadAllLines(peoplePath);

                     Matching matching = new Matching(people, advertisers);
                     matching.PrintMaxMatching();
                }
            }
        }
    }
}
